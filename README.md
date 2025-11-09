# 📦 Real-Time Order Tracking System

A beginner-friendly **ASP.NET Core Web API** project demonstrating **Apache Kafka** integration for real-time event-driven architecture. This system simulates an e-commerce order processing pipeline where orders are published to Kafka and processed asynchronously by background consumers.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Kafka](https://img.shields.io/badge/Apache%20Kafka-2.3.0-231F20?logo=apachekafka)](https://kafka.apache.org/)
[![Docker](https://img.shields.io/badge/Docker-Required-2496ED?logo=docker)](https://www.docker.com/)

---

## 🎯 **Project Goals**

This project is designed for **learning purposes** to understand:

✅ Kafka Producer/Consumer basics  
✅ Event-driven architecture  
✅ ASP.NET Core Web API development  
✅ Clean architecture with separation of concerns  
✅ Background services in .NET  
✅ Async/await patterns  
✅ RESTful API design with Swagger documentation  

---

## 🏗️ **Architecture Overview**

```
┌─────────────┐      HTTP      ┌──────────────────┐
│   Browser   │ ─────────────> │   Web API        │
│  (Swagger)  │                │  (Port 5000)     │
└─────────────┘                └──────────────────┘
                                        │
                                        │ Publish
                                        ▼
                               ┌──────────────────┐
                               │  Kafka Producer  │
                               └──────────────────┘
                                        │
                                        ▼
                               ┌──────────────────┐
                               │   Kafka Broker   │
                               │   (Port 9092)    │
                               │  Topic: orders   │
                               └──────────────────┘
                                        │
                                        │ Subscribe
                                        ▼
                               ┌──────────────────┐
                               │  Kafka Consumer  │
                               │  (Background)    │
                               └──────────────────┘
                                        │
                                        │ Save
                                        ▼
                               ┌──────────────────┐
                               │   Repository     │
                               │   (In-Memory)    │
                               └──────────────────┘
```



## **Getting Started**

### **Prerequisites**

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- A code editor ([VS Code](https://code.visualstudio.com/) or [Visual Studio](https://visualstudio.microsoft.com/))

### **Installation**

1. **Clone the repository**
   ```bash
   git clone <your-repo-url>
   cd OrderTrackingSystem
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   ```

3. **Start Kafka with Docker**
   ```bash
   docker-compose up -d
   ```
   
   This will start:
   - Zookeeper (port 2181)
   - Kafka Broker (port 9092)

4. **Verify Kafka is running**
   ```bash
   docker ps
   ```
   You should see both `zookeeper` and `kafka` containers running.

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access Swagger UI**
   
   Open your browser and navigate to:
   ```
   http://localhost:5000
   ```

---

## **Testing the API**

### **Using Swagger UI (Recommended)**

1. Open `http://localhost:5000`
2. Click on **POST /api/orders**
3. Click **"Try it out"**
4. Use this sample JSON:
   ```json
   {
     "userId": 1,
     "productName": "Gaming Laptop",
     "quantity": 2,
     "price": 1299.99
   }
   ```
5. Click **"Execute"**
6. You should see a `200 OK` response with the created order

### **Using cURL**

```bash
# Create an order
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 1,
    "productName": "Gaming Laptop",
    "quantity": 2,
    "price": 1299.99
  }'

# Get all orders
curl http://localhost:5000/api/orders

# Get specific order
curl http://localhost:5000/api/orders/{orderId}

# Get orders by user
curl http://localhost:5000/api/orders/user/1

# Get orders by status
curl http://localhost:5000/api/orders/status/COMPLETED
```

### **Using PowerShell**

```powershell
# Create an order
Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/orders" `
  -ContentType "application/json" `
  -Body '{"userId":1,"productName":"Gaming Laptop","quantity":2,"price":1299.99}'

# Get all orders
Invoke-RestMethod -Method Get -Uri "http://localhost:5000/api/orders"
```

---

## 📊 **API Endpoints**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/orders` | Create a new order |
| `GET` | `/api/orders` | Get all orders |
| `GET` | `/api/orders/{orderId}` | Get order by ID |
| `GET` | `/api/orders/user/{userId}` | Get orders by user |
| `GET` | `/api/orders/status/{status}` | Get orders by status |

---

## 🔄 **How It Works**

### **Order Creation Flow**

1. **Client** sends POST request to `/api/orders`
2. **OrdersController** receives the request
3. **OrderService** creates an Order with status `PLACED`
4. **KafkaProducerService** publishes order to Kafka topic `orders`
5. **API** returns success response immediately (async processing)

### **Background Processing Flow**

1. **KafkaConsumerService** (background worker) listens to `orders` topic
2. Receives order message from Kafka
3. Updates order status to `PROCESSING`
4. Saves to repository
5. Simulates processing (200ms delay)
6. Updates order status to `COMPLETED`
7. Logs completion

### **Order Retrieval Flow**

1. **Client** sends GET request
2. **OrderService** queries the repository
3. **Repository** returns orders from in-memory storage
4. **API** returns formatted response

---

## 🛠️ **Configuration**

### **Kafka Settings** (`appsettings.json`)

```json
{
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "ConsumerGroupId": "order-consumer-group"
  }
}
```

---

## 📝 **Monitoring Kafka**

### **View Kafka Topics**

```bash
docker exec -it kafka kafka-topics --bootstrap-server localhost:9092 --list
```

### **View Messages in 'orders' Topic**

```bash
docker exec -it kafka kafka-console-consumer \
  --bootstrap-server localhost:9092 \
  --topic orders \
  --from-beginning
```

### **Create Topic Manually** (optional)

```bash
docker exec -it kafka kafka-topics \
  --bootstrap-server localhost:9092 \
  --create \
  --topic orders \
  --partitions 1 \
  --replication-factor 1
```

---

## 🐛 **Troubleshooting**

### **Port 5000 Already in Use**

```bash
# Change port in Program.cs
builder.WebHost.UseUrls("http://localhost:5001");
```

### **Kafka Connection Error**

```bash
# Check if Kafka is running
docker ps

# Check Kafka logs
docker logs kafka

# Restart Kafka
docker-compose restart kafka
```

### **Consumer Not Receiving Messages**

- The topic is created automatically when the first message is published
- Wait a few seconds after sending the first order
- Check console logs for consumer activity

### **Swagger Not Loading**

- Ensure the app is running: `dotnet run`
- Check the correct URL in console output
- Try `http://localhost:5000` or the URL shown in logs

---

## 🎓 **Learning Outcomes**

By building and running this project, you'll learn:

- ✅ **Kafka Fundamentals**: Producers, consumers, topics, and brokers
- ✅ **Event-Driven Architecture**: Decoupling services with message queues
- ✅ **ASP.NET Core**: Web API development with controllers and services
- ✅ **Clean Architecture**: Separation of concerns (Controllers, Services, Data)
- ✅ **Background Services**: Using `IHostedService` for background workers
- ✅ **Dependency Injection**: Managing service lifetimes and scopes
- ✅ **Async Programming**: Proper use of `async/await`
- ✅ **API Documentation**: Swagger/OpenAPI integration
- ✅ **Docker**: Containerizing infrastructure services

---

## 🔮 **Future Enhancements**

Want to take this project further? Try these:

- [ ] Replace in-memory storage with **PostgreSQL** or **MongoDB**
- [ ] Add **order status notifications** using SignalR
- [ ] Implement **dead letter queues** for failed messages
- [ ] Add **distributed tracing** with OpenTelemetry
- [ ] Create **multiple consumer instances** for scalability
- [ ] Add **authentication and authorization** (JWT)
- [ ] Implement **retry policies** with Polly
- [ ] Add **unit and integration tests**
- [ ] Deploy to **Kubernetes** or **Azure**

---

## 📚 **Resources**

- [Apache Kafka Documentation](https://kafka.apache.org/documentation/)
- [Confluent.Kafka .NET Client](https://docs.confluent.io/kafka-clients/dotnet/current/overview.html)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core/)
- [Background tasks with hosted services](https://docs.microsoft.com/aspnet/core/fundamentals/host/hosted-services)

---

## 🤝 **Contributing**

Contributions are welcome! Feel free to:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 👤 **Author**

**Your Name**
- GitHub: [@BrianKimurgor](https://github.com/BrianKimurgor)
- Email: kimurgorbrian20@gmail.com

---

