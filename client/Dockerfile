# 使用 Node.js 18 作为构建环境
FROM node:18 AS build

WORKDIR /app

# 复制 package.json 和 package-lock.json (如果存在)
COPY package*.json ./

# 安装依赖
RUN npm install

# 复制所有前端源代码
COPY . .

# 构建 React 应用
RUN npm run build

# 使用 Nginx Alpine 作为运行时环境
FROM nginx:alpine

# 从构建阶段复制 dist 目录到 Nginx 默认目录
COPY --from=build /app/dist /usr/share/nginx/html

# 复制自定义 nginx 配置
COPY nginx.conf /etc/nginx/conf.d/default.conf

# 暴露端口 80
EXPOSE 80

# 启动 Nginx
CMD ["nginx", "-g", "daemon off;"]
