// TcpClientSslServer.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#define WIN32_LEAN_AND_MEAN

#include <windows.h>
#include <winsock2.h>
#include <ws2tcpip.h>
#include <stdlib.h>
#include <stdio.h>


// Need to link with Ws2_32.lib, Mswsock.lib, and Advapi32.lib
#pragma comment (lib, "Ws2_32.lib")
#pragma comment (lib, "Mswsock.lib")
#pragma comment (lib, "AdvApi32.lib")


#pragma comment(lib,"ws2_32.lib")//ssl support
#pragma comment(lib,"libssl.lib")//ssl support
#pragma comment(lib,"libcrypto.lib")//ssl support
#include <openssl/ssl.h>//ssl support
#include <openssl/err.h>//ssl support

#ifdef _MSC_VER
#define _CRT_SECURE_NO_WARNINGS
#endif
#pragma warning(disable:4996)


#include <string>

static std::string appDir;

static SOCKET m_socket = INVALID_SOCKET;
static SSL* m_ssl_socket = NULL;
static int sslSocketCount = 0;

void getSslErrors(TCHAR* m, int size)
{
	BIO *bio = BIO_new(BIO_s_mem());
	ERR_print_errors(bio);
	//char buffer[2000];
	char* buffer = NULL;
	size_t bl = BIO_get_mem_data(bio, &buffer);
	size_t string_length = bl < size - 1 ? bl : size - 1;
#ifdef UNICODE
	//TCHAR == WCHAR
	mbstowcs(m, buffer, string_length);
#else
	//TCHAR == char	
	memcpy(m, buffer, string_length);
#endif
	m[string_length] = '\0';
	BIO_free(bio);
}

void throwSslException()
{
	TCHAR m[2000];
	getSslErrors(m, sizeof(m));
	throw m;
}

SSL_CTX* createSslContext(bool server)
{
	const SSL_METHOD* method = server ? SSLv23_server_method() : SSLv23_client_method();
	SSL_CTX* ctx = SSL_CTX_new(method);
	if (ctx == NULL)
		throwSslException();

	if (!server)
		return ctx;

	//#####################create self-signed certificate and key######################
	//>openssl.exe req -newkey rsa:2048 -config cnf/openssl.cnf  -nodes -keyout key.pem -x509 -days 365 -out certificate.pem

	SSL_CTX_set_ecdh_auto(ctx, 1);

	/*const long flags = SSL_OP_NO_SSLv2 | SSL_OP_NO_SSLv3 | SSL_OP_NO_COMPRESSION;
	SSL_CTX_set_options(ctx, flags);*/

	char buffer[2000];
	sprintf(buffer, "%s\\%s", appDir.c_str(), "server_certificate.pem");
	if (SSL_CTX_use_certificate_file(ctx, buffer, SSL_FILETYPE_PEM) <= 0)
		throwSslException();

	sprintf(buffer, "%s\\%s", appDir.c_str(), "server_key.pem");
	if (SSL_CTX_use_PrivateKey_file(ctx, buffer, SSL_FILETYPE_PEM) <= 0)
		throwSslException();

	return ctx;
}

static int send(const char *data, int size, int flags)
{
	int result;

	if (m_ssl_socket != NULL)
	{
		result = SSL_write(m_ssl_socket, data, size);
		if (result == 0)
			throw "Connection has been gracefully closed";
		if (result < 0)
			throw "Failed to write data to SSL socket.";
	}
	else
	{
		result = ::send(m_socket, data, size, flags);
		if (result == -1)
			throw "Failed to send data to socket.";
	}

	return result;
}

static int recv(char *buffer, int size, int flags)
{
	int result;

	if (m_ssl_socket != NULL)
	{
		result = SSL_read(m_ssl_socket, buffer, size);
		if (result == 0)
			throw "Connection has been gracefully closed";
		if (result < 0)
			throw "Failed to read data from SSL socket.";
	}
	else
	{
		result = ::recv(m_socket, buffer, size, flags);
		if (result == 0)
			throw "Connection has been gracefully closed";
		if (result == SOCKET_ERROR)
			throw "Failed to recv data from socket.";
	}
	return result;
}

static int sendAll(const char *data, int size, int flags)
{
	int r = 0;
	while (r < size)
		r += send(&data[r], size - r, flags);
	return r;
}

static int recvAll(char *buffer, int size, int flags)
{
	int r = 0;
	while (r < size)
		r += recv(&buffer[r], size - r, flags);
	return r;
}

static SSL_CTX* m_sslCtx = NULL;
static bool sslInitialized = false;
void initializeSsl(bool server)
{
	if (!sslInitialized)
	{
		sslInitialized = true;
		ERR_load_crypto_strings();
		SSL_load_error_strings();
		//SSL_library_init();
		OpenSSL_add_ssl_algorithms();
		//OPENSSL_config(NULL);
	}
	if (m_sslCtx == NULL)
		m_sslCtx = createSslContext(server);
}

void createSslSocket(bool server)
{
	if (m_ssl_socket != NULL)
		return;

	sslSocketCount++;
	if (sslSocketCount == 1)
		initializeSsl(server);

	m_ssl_socket = SSL_new(m_sslCtx);
	if (m_ssl_socket == NULL)
		throwSslException();
	SSL_set_fd(m_ssl_socket, m_socket);
	if (server)
	{
		if (SSL_accept(m_ssl_socket) <= 0)
			throwSslException();
	}
	else
	{
		if (SSL_connect(m_ssl_socket) <= 0)
			throwSslException();
	}
}

static void shutdownSsl()
{
	//Not really necessary, since resources will be freed on process termination
	ERR_free_strings();
	//RAND_cleanup();
	EVP_cleanup();
	// this seems to cause double-frees for me: ENGINE_cleanup ();
	//CONF_modules_free();
	ERR_remove_state(0);
	sslInitialized = false;

	if (m_sslCtx != NULL)
	{
		SSL_CTX_free(m_sslCtx);
		m_sslCtx = NULL;
	}
}

void destroySslSocket()
{
	if (m_ssl_socket == NULL)
		return;

	SSL_free(m_ssl_socket);
	m_ssl_socket = NULL;
	sslSocketCount--;
	if (sslSocketCount == 0)
		shutdownSsl();
}

#define DEFAULT_BUFLEN 512
#define DEFAULT_IP "127.0.0.1"
#define DEFAULT_PORT "5900"


int __cdecl main(int argc, char **argv)
{
	std::string argv_str(argv[0]);
	appDir = argv_str.substr(0, argv_str.find_last_of("\\"));

	WSADATA wsaData;
	struct addrinfo *result = NULL,
		*ptr = NULL,
		hints;
	char recvbuf[DEFAULT_BUFLEN];
	int iResult;
	int recvbuflen = DEFAULT_BUFLEN;

	// Initialize Winsock
	iResult = WSAStartup(MAKEWORD(2, 2), &wsaData);
	if (iResult != 0) {
		printf("WSAStartup failed with error: %d\n", iResult);
		return 1;
	}

	ZeroMemory(&hints, sizeof(hints));
	hints.ai_family = AF_UNSPEC;
	hints.ai_socktype = SOCK_STREAM;
	hints.ai_protocol = IPPROTO_TCP;

	// Resolve the server address and port
	iResult = getaddrinfo(DEFAULT_IP, DEFAULT_PORT, &hints, &result);
	if (iResult != 0) {
		printf("getaddrinfo failed with error: %d\n", iResult);
		WSACleanup();
		return 1;
	}

	// Attempt to connect to an address until one succeeds
	for (ptr = result; ptr != NULL; ptr = ptr->ai_next) {

		// Create a SOCKET for connecting to server
		m_socket = socket(ptr->ai_family, ptr->ai_socktype,
			ptr->ai_protocol);
		if (m_socket == INVALID_SOCKET) {
			printf("socket failed with error: %ld\n", WSAGetLastError());
			WSACleanup();
			return 1;
		}

		// Connect to server.
		iResult = connect(m_socket, ptr->ai_addr, (int)ptr->ai_addrlen);
		if (iResult == SOCKET_ERROR) {
			closesocket(m_socket);
			m_socket = INVALID_SOCKET;
			continue;
		}
		break;
	}

	freeaddrinfo(result);

	if (m_socket == INVALID_SOCKET) {
		printf("Unable to connect to server!\n");
		WSACleanup();
		return 1;
	}

	{
		char message_length[2] = { 9,0 };
		sendAll(message_length, 2, 0);
		sendAll("SslStart\0", 9, 0);
		recvAll(recvbuf, 2, 0);
		recvAll(recvbuf, recvbuf[0], 0);
	}

	if(recvbuf[9] == 'O' &&recvbuf[10] == 'K')//check if the command is OK
		createSslSocket(true);

	/*recvAll(recvbuf, 2, 0);
	recvAll(recvbuf, recvbuf[0], 0);*/

	{
		char message_length[2] = { 11,0 };
		sendAll(message_length, 2, 0);
		sendAll("FfmpegStop\0", 11, 0);
		recvAll(recvbuf, 2, 0);
		recvAll(recvbuf, recvbuf[0], 0);
	}

	// cleanup
	closesocket(m_socket);
	WSACleanup();

	return 0;
}