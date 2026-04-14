# 1. Sử dụng image SDK để build ứng dụng
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file .csproj và restore các thư viện (packages)
COPY ["GundamStoreAPI.csproj", "./"]
RUN dotnet restore "./GundamStoreAPI.csproj"

# Copy toàn bộ code và build bản Release
COPY . .
RUN dotnet publish "GundamStoreAPI.csproj" -c Release -o /app/publish

# 2. Sử dụng image ASP.NET runtime nhẹ hơn để chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Render mặc định sẽ tự tìm port, nhưng cấu hình tường minh ở 8080 cho .NET 8 là chuẩn nhất
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GundamStoreAPI.dll"]