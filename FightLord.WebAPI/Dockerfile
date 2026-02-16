# 使用 .NET 10.0 SDK 作为构建环境
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 复制 csproj 文件并还原依赖
COPY ["FightLord.WebAPI/FightLord.WebAPI.csproj", "FightLord.WebAPI/"]
COPY ["FightLord.Infrastructure/FightLord.Infrastructure.csproj", "FightLord.Infrastructure/"]
COPY ["FightLord.Application/FightLord.Application.csproj", "FightLord.Application/"]
COPY ["FightLord.Core/FightLord.Core.csproj", "FightLord.Core/"]

RUN dotnet restore "FightLord.WebAPI/FightLord.WebAPI.csproj"

# 复制所有源代码并构建
COPY . .
WORKDIR "/src/FightLord.WebAPI"
RUN dotnet build "FightLord.WebAPI.csproj" -c Release -o /app/build

# 发布应用
FROM build AS publish
RUN dotnet publish "FightLord.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 使用 .NET 10.0 ASP.NET Core Runtime 作为运行环境
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_HTTP_PORTS=80
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FightLord.WebAPI.dll"]
