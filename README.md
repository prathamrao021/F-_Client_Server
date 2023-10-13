# Concurrent Socket Communication in F#

This repository contains the source code for a concurrent client/server implementation for socket communication using F#.

## Description

The implementation consists of two main components:

1. **Server Program:** The server program, written in F#, listens for incoming client connections and responds to their requests. It handles multiple clients simultaneously.

2. **Client Programs:** Client program, also written in F#, can run concurrently and establish connections to the server. They send messages containing arithmetic calculation commands (e.g., add, subtract, multiply) along with input numbers (e.g., add 12 7) to the server. The server calculates the result and responds to each client with the computed value.

## Usage

To use this code for your own educational purposes or to complete the assignment, follow these steps:

1. Clone the repository to your local machine:

   ```bash
   git clone https://github.com/pnkjwrk3/FSharp-TCP-Client-Server.git
2. Navigate to the project directory:
   
   ```bash
   cd FSharp-TCP-Client-Server
3. Navigate to the 'server' folder and execute the following commands to build the server project:
   
   ```bash
    cd server
    dotnet build
4. Navigate to the 'client' folder and execute the following commands to build the client project:
   
   ```bash
   cd ../client
   dotnet build
5. Run the server program by executing the following command within the 'server' folder:

   ```bash
   dotnet bin/Debug/net7.0/server.dll
6. To start clients, navigate to the 'client' folder and execute the following command:
   
   ```bash
   dotnet bin/Debug/net7.0/client.dll
   ```
   You can run multiple client instances simultaneously to establish connections to the server.

