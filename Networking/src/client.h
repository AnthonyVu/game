#include <netinet/in.h>
#include <arpa/inet.h>
#include <map>
#include "Connection.h"
#include <poll.h>
#include <stdio.h>


#define SERVER "142.232.135.38"
#define SOCKET_DATA_WAITING 555
#define SOCKET_NODATA 666


class Client {

public:
	Client();
	int initializeSocket(short port);
	void sendBytes(char * data, unsigned len);
	int32_t receiveBytes(char * buffer, uint32_t size);
	int32_t UdpPollSocket();
private:
	int clientSocket;
	sockaddr_in serverAddr;


};