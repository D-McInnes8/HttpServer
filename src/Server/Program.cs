using Application;

var tcpServer = new TcpServer(9999);
await tcpServer.StartAsync();