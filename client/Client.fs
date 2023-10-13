// Client program

open System
open System.Net.Sockets
open System.Text
open System.Threading

// Define the server address and port number
let address = "127.0.0.1"
let port = 8888

// Create a TCP client object
let client = new TcpClient()

// Connect to the server
client.Connect(address, port)
printfn "Connected to server at %s:%d" address port

// Get the network stream from the client
let stream = client.GetStream()

// Function to send a command to the server and receive the response
let sendCommand (command: string) =
    // Encode the message as bytes and write it to the stream
    stream.Write(Encoding.UTF8.GetBytes(command), 0, command.Length)
    printfn "Sent to server: %s" command

    // Create a buffer to store incoming data
    let buffer = Array.zeroCreate 1024

    // Read data from the stream and decode it as a string
    let bytes = stream.Read(buffer, 0, buffer.Length)
    let response = Encoding.UTF8.GetString(buffer, 0, bytes)
    printfn "Received from server: %s" response
    response

// // Function to read and print the initial message from the server

// Function to continuously check for incoming messages from the server
let rec checkForIncomingMessages () =
    let buffer = Array.zeroCreate 1024

    // Continuously check for incoming messages
    while true do
        // Check if there is data available for reading from the stream
        if stream.DataAvailable then
            // Read data from the stream and decode it as a string
            let bytes = stream.Read(buffer, 0, buffer.Length)
            let response = Encoding.UTF8.GetString(buffer, 0, bytes)
            printfn "Received from server: %s" response

            // Process the received message (if needed)
            if response = "-5" then
                // Exit the client and close
                printfn "#exit"
                stream.Close()
                client.Close()
                Environment.Exit(0)

        // Sleep for a short duration to avoid busy-waiting
        Thread.Sleep(100)

// Start a separate thread to check for incoming messages from the server
let incomingMessagesThread = Thread(fun () -> checkForIncomingMessages ())
incomingMessagesThread.Start()

// Function to interact with the server
let rec interactWithServer () =
    // Prompt the user for a command
    // printf "Enter a command and inputs (e.g. add 12 7) or exit to quit: "
    let command = Console.ReadLine()

    
    // Send the command to the server and receive the response
    let response = sendCommand command

    // Check if the response is an error code
    // match Int32.TryParse(response) with
    // | true, errorCode ->
    // Check if the response is a negative number (indicating an error code)
    if response.StartsWith("-") then
        // Handle error codes
        match response with
        | "-1" -> printfn "Error: Incorrect operation command"
        | "-2" -> printfn "Error: Number of inputs is less than two"
        | "-3" -> printfn "Error: Number of inputs is more than four"
        | "-4" -> printfn "Error: One or more inputs contain non-number(s)"
        | "-5" -> 
            // Print "#exit" and exit the client
            printfn "#exit"
            // Close the stream and the client
            stream.Close()
            client.Close()
            // printfn "Disconnected from server"
            Environment.Exit(0)
        | _ -> printfn "Error: Unknown error code"
    else
        // Print the result from the server
        printfn "Result: %s" response

    // Continue interacting with the server
    interactWithServer()

// Start interacting with the server
interactWithServer()
