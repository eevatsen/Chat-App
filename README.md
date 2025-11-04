# Real-time Chat Application with Sentiment Analysis

## **Live Demo**

👉 **Visit the live application here** https://proud-island-01049aa03.3.azurestaticapps.net/

---

## **About The Project**

This is a full-stack, real-time chat application. The primary goal is to build a robust, scalable, and cloud-native chat service utilizing a modern .NET and Azure technology stack.

The application allows users to send and receive messages instantly. It also integrates **Azure Cognitive Services** to perform real-time sentiment analysis on each message, providing immediate visual feedback on the message's tone.

---

## **Features**

* **Instant Messaging:** Blazing-fast message delivery to all connected clients using **Azure SignalR Service**.
* **Sentiment Analysis:** Every message is analyzed by **Azure Text Analytics**. The sentiment (positive, negative, neutral) is displayed with an emoji (😊, 😐, 😠).
* **Chat History:** Past messages are persisted and loaded from an **Azure SQL Database**.
* **Animated UI:** New messages appear with CSS animations based on their sentiment (a pulse for positive, a shake for negative).
* **Cloud-Native Deployment:** Fully deployed on **Azure** using a decoupled frontend/backend architecture.

---

## **Tech Stack & Azure Architecture**

This project uses a decoupled architecture, with the frontend and backend deployed and scaled independently.

### **Frontend (Client)**

* **Framework:** Angular
* **Technology:** TypeScript, SignalR Client
* **Hosting:** Azure Static Web App
* **Routing:** A `staticwebapp.config.json` file is used to proxy API requests (`/api/*` and `/chatHub/*`) from the frontend to the backend App Service, bypassing browser CORS restrictions.

### **Backend (Server)**

* **Framework:** .NET 8 (ASP.NET Core Web API)
* **Hosting:** Azure App Service
* **Real-time:** Azure SignalR Service handles all WebSocket connections, allowing the App Service to remain stateless and scale efficiently.
* **Database:** Azure SQL Database
* **Database Authentication:** Entra ID-only Authentication. The App Service uses its **Managed Identity** to securely authenticate with the SQL server, eliminating the need for passwords in configuration.
* **AI / Sentiment:** Azure Cognitive Services (**Text Analytics**) is called by the backend to analyze messages before they are saved and broadcast.

---

## **Local Development Setup**

### **1. Run the Backend**

* Navigate to the backend project:

  ```bash
  cd server/ChatApp.Server/ChatApp.Server
  ```
* Configure your **appsettings.Development.json** [cite: appsettings.Development.json] with a local connection string (e.g., to SQL Server LocalDB or a local PostgreSQL instance).
* In **Program.cs** [cite: eevatsen/chat-app/Chat-App-573d47fcaa8b82bd00c569f42e56fe5a2df8b72e/server/ChatApp.Server/ChatApp.Server/Program.cs], comment out `.AddAzureSignalR()` and ensure `builder.Services.AddSignalR();` is used for self-hosted SignalR.
* Apply your database migrations:

  ```bash
  dotnet ef database update
  ```
* Run the backend:

  ```bash
  dotnet run
  ```

### **2. Run the Frontend**

* Navigate to the client project:

  ```bash
  cd client
  ```
* Install dependencies:

  ```bash
  npm install
  ```
* Ensure your **proxy.conf.json** [cite: eevatsen/chat-app/Chat-App-573d47fcaa8b82bd00c569f42e56fe5a2df8b72e/client/proxy.conf.json] `target` field points to your local backend's URL (e.g., `https://localhost:7038`).
* In **chat.service.ts** [cite: eevatsen/chat-app/Chat-App-573d47fcaa8b82bd00c569f42e56fe5a2df8b72e/client/src/app/services/chat.service.ts], ensure you are using relative paths (`/chatHub`, `/api/messages`) so the proxy is used.
* Run the frontend:

  ```bash
  npm start
  ```
* Open the application in your browser at:
  👉 **[http://localhost:4200/](http://localhost:4200/)**
