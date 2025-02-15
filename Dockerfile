# Используем .NET SDK 6.0 для сборки приложения
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Копируем файлы проекта и выполняем сборку
COPY . .
RUN dotnet publish -c Release -o out

# Используем ASP.NET Core Runtime 6.0 для запуска приложения
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

# Копируем собранные файлы из предыдущего этапа
COPY --from=build /app/out .

# Задаем команду для запуска приложения
ENTRYPOINT ["dotnet", "Misha's bot.dll"]

COPY /voices /app/voices