// Server program

open System
open System.Net
open System.Net.Sockets
open System.Text

// Define a port number for the server to listen on
let port = 8888

// Create a TCP listener object
let listener = new TcpListener(IPAddress.Any, port)

// Start listening for incoming connections
listener.Start()
printfn "Server is listening on port %d" port

// Create a list to store the active clients
// let clients = ResizeArray<TcpClient>()
type ClientInfo = { Client: TcpClient; Number: int }

let clients = ResizeArray<ClientInfo>()
let mutable nextClientNumber = 1

// Define a function to check if a string is a valid integer
let isInteger (s: string) =
    // Try to parse the string as an integer and return true if successful, false otherwise
    match Int32.TryParse(s) with
    | (true, _) -> true
    | (false, _) -> false

// Define a function to close all the clients and exit the server gracefully
let terminate () =
    let mutable disconnectedClients = []
    for clientInfo in clients do
        try
            let stream = clientInfo.Client.GetStream()
            if clientInfo.Client.Connected then
                stream.Write(Encoding.UTF8.GetBytes("-5"), 0, 2)
                stream.Close()
                clientInfo.Client.Close()
                printfn "Sent from terminate client %d: -5" clientInfo.Number
            else
                disconnectedClients <- clientInfo :: disconnectedClients
        with
        | ex ->
            printfn "Error while terminating client: %s" (ex.Message)

    // Remove disconnected clients from the list
    for disconnectedClient in disconnectedClients do
        clients.Remove(disconnectedClient)
        
    // Clear the list of active clients
    clients.Clear()

    // Stop listening for incoming connections
    listener.Stop()
    printfn "Server is stopped"
    Environment.Exit(0)

    

// Define a function to check if a command is valid and return the corresponding error code or result
let checkCommand (command: string) (inputs: int array) =
    try
        // Check if the command is one of the valid operations
        match command with
        | "add" | "sub" | "mul" ->
            // Check if the number of inputs is between 2 and 4
            match inputs.Length with
            | 2 | 3 | 4 ->
                // Perform the arithmetic calculation based on the command
                let result =
                    match command with
                    | "add" -> inputs |> Array.sum
                    | "sub" -> inputs |> Array.reduce (-)
                    | "mul" -> inputs |> Array.reduce (*)
                    | _ -> failwith "Invalid command"
                // Return the result as a positive integer
                if result < 0 then -1 else result
            | n when n < 2 ->
                // Return -2 as the error code for less than 2 inputs
                -2
            | n when n > 4 ->
                // Return -3 as the error code for more than 4 inputs
                -3
        | "bye" ->
            // Return -5 as the error code for exit
            -5
        | "terminate" ->
            // Call the terminate function and return -5 as the error code for exit
            terminate (); -5
        | _ ->
            // Return -1 as the error code for invalid operation command
            -1
    with
    | ex ->
        // Handle other exceptions by returning -1 as the error code
        -1

// Define a function to handle each client connection
let handleClient (clientInfo: ClientInfo) =
    // get client
    let client = clientInfo.Client

    // Get the network stream from the client
    let stream = client.GetStream()

    // Create a buffer to store incoming data
    let buffer = Array.zeroCreate 1024

    // Write a message to the client to greet them with "Hello!"
    stream.Write(Encoding.UTF8.GetBytes("Hello!"), 0, 6)
    printfn "Sent to client %d: Hello!" clientInfo.Number

    // Loop until the client closes the connection
    let rec loop () =
        // Read data from the stream
        let bytes = stream.Read(buffer, 0, buffer.Length)

        // Check if any data was received
        if bytes > 0 then
            // Decode the data as a string
            let message = Encoding.UTF8.GetString(buffer, 0, bytes)
            printfn "Received from client %d: %s" clientInfo.Number message

            // Parse the message as a command and inputs
            let parts = message.Split(' ')
            let command = parts.[0]
            let inputs = parts.[1..]

            // Check if all the inputs are valid integers using the isInteger function
            let validInputs = inputs |> Array.forall isInteger

            // Declare a variable to store the result or error code
            let resultOrError =
                // Check if the inputs are valid integers
                if validInputs then 
                    // Convert the inputs to an array of integers using Int32.Parse function
                    let inputNumbers = inputs |> Array.map Int32.Parse
                    try
                        // Use the checkCommand function to validate the command and perform the calculation or termination 
                        checkCommand command inputNumbers 
                    with
                    | :? System.FormatException ->
                        // Return -4 as the error code for one or more inputs containing non-number(s)
                        -4
                    | ex ->
                        // Return -1 as the error code for unexpected exceptions
                        -1
                else 
                    // Return -4 as the error code for invalid input(s)
                    -4

            // Encode the result or error code as a string
            let response = resultOrError.ToString()
            if command = "terminate" && resultOrError = -5 then
                false
            else
                // Write the response to the stream
                stream.Write(Encoding.UTF8.GetBytes(response), 0, response.Length)
                printfn "Sent to client %d: %s" clientInfo.Number response

                // Check if the result or error code is positive or -5 (exit)
                if resultOrError > 0 then
                    // Continue the loop 
                    loop ()
                elif resultOrError = -5 && command <> "terminate" then
                    // Close the stream and the client
                    stream.Close()
                    client.Close()
                    printfn "Client %d disconnected, bye" clientInfo.Number

                    // Remove the client from the list of active clients 
                    clients.Remove(clientInfo)
                elif resultOrError = -1 || resultOrError = -2 || resultOrError = -3 || resultOrError = -4 then
                    // Handle error codes -1 (incorrect operation command) and -4 (invalid input)
                    loop ()
                elif resultOrError = -5 && command = "terminate" then
                    // Break the loop and close all the clients and exit the server 
                    false
                else
                    // Break the loop and close all the clients and exit the server 
                    // terminate()
                    false
        else
            // Close the stream and the client
            // stream.Close()
            // client.Close()
            printfn "Client %d disconnected" clientInfo.Number
            false

            // Remove the client from the list of active clients
            // clients.Remove(client)

    // Start the loop
    loop ()


// Loop until the server is stopped
try
    while true do
        // Accept an incoming connection from a client
        let client = listener.AcceptTcpClient()
        printfn "Client connected"

        // Add the client to the list of active clients
        // let clientNumber = clients.Count + 1
        let clientNumber = nextClientNumber
        nextClientNumber <- nextClientNumber + 1
        
        let clientInfo = { Client = client; Number = clientNumber }
        clients.Add(clientInfo)
        // clients.Add(client)
        // Print the IP address of the new client
        let clientEndPoint = client.Client.RemoteEndPoint :?> IPEndPoint
        printfn "New client %d connected: %s" clientNumber (clientEndPoint.Address.ToString())
        printfn "Number of clients connected: %d" clients.Count

        // Handle the client connection in a separate thread
        Async.Start (async { handleClient clientInfo })
with
| :? System.Net.Sockets.SocketException as sockEx when sockEx.SocketErrorCode = System.Net.Sockets.SocketError.Interrupted ->
    // Handle the interruption gracefully
    printfn "Server interrupted. Cleaning up..."
    terminate ()
| ex ->
    // Handle other exceptions
    printfn "Server error: %s" ex.Message
