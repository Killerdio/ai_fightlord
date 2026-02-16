# 仅练手ai编程使用
新人使用trae体验vibe coding做的开源项目，仅供各位参考学习，请勿商用

# FightLord (斗地主)

FightLord 是一个基于现代 Web 技术构建的多人在线斗地主游戏。项目采用前后端分离架构，后端使用 .NET 10 和 SignalR 实现实时通信，前端使用 React 18 构建用户界面。

## 🛠️ 技术栈

- **后端**: C# .NET 10
- **前端**: React 18, Vite
- **实时通信**: SignalR
- **缓存**: Redis
- **容器化**: Docker & Docker Compose

## 📋 先决条件

在开始之前，请确保您的开发环境已安装以下工具：

- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Node.js](https://nodejs.org/) (v18+)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## 🚀 本地开发步骤

### 1. 后端 (WebAPI)

后端服务默认运行在 `http://localhost:5000` (需配置或使用 Docker) 或 `http://localhost:5197` (默认 `dotnet run`)。
**注意**: 前端代理默认指向 `http://localhost:5000`，建议启动时指定端口。

```bash
cd FightLord.WebAPI
# 建议指定端口以匹配前端代理配置
dotnet run --urls "http://localhost:5000"
```

### 2. 前端 (Client)

前端开发服务器默认运行在 `http://localhost:5173`。

```bash
cd client
npm install
npm run dev
```

打开浏览器访问 `http://localhost:5173` 即可开始游戏。

## 🐳 Docker 部署步骤

使用 Docker Compose 可以一键启动所有服务（后端、前端、Redis）。

1. **配置环境变量**

   ```bash
   cp .env.example .env
   ```

2. **启动服务**

   ```bash
   docker-compose up -d
   ```

3. **访问服务**

   - **前端**: `http://localhost` (端口 80)
   - **后端 API**: `http://localhost:5000`

## 📚 API 文档

后端启动后，您可以访问 Swagger UI 查看完整的 API 文档：

- **Local**: [http://localhost:5000/swagger](http://localhost:5000/swagger)
- **Docker**: [http://localhost:5000/swagger](http://localhost:5000/swagger)

## 🔌 WebSocket (SignalR) 事件文档

游戏核心逻辑通过 REST API 触发，状态更新通过 WebSocket (SignalR) 实时推送。

**Hub Endpoint**: `/gameHub`

### 客户端调用方法 (Client -> Server)

虽然主要游戏操作通过 API (POST) 进行，但以下方法可通过 Hub 直接调用：

| 方法名 | 参数 | 描述 |
|--------|------|------|
| `JoinRoom` | `roomId` (string) | 加入指定房间组，接收该房间的推送消息。 |
| `LeaveRoom` | `roomId` (string) | 离开指定房间组。 |
| `SendMessage` | `roomId` (string), `user` (string), `message` (string) | 在房间内发送聊天消息。 |

### 服务器推送事件 (Server -> Client)

客户端应监听以下事件以更新 UI：

| 事件名 | 参数 | 描述 |
|--------|------|------|
| `UserJoined` | `connectionId` (string) | 当有新用户加入房间时触发。 |
| `UserLeft` | `connectionId` (string) | 当用户离开房间时触发。 |
| `ReceiveMessage` | `user` (string), `message` (string) | 接收到房间内的聊天消息。 |
| `GameUpdated` | `gameState` (object) | 游戏状态更新（如出牌、叫分、过牌后），包含最新游戏数据。 |

### 游戏操作 API (REST)

游戏动作通过 HTTP POST 请求触发，成功后服务器会广播 `GameUpdated` 事件。

- **出牌**: `POST /api/v1/game/play`
- **叫分**: `POST /api/v1/game/bid`
- **过牌**: `POST /api/v1/game/pass`
