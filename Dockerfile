FROM mcr.microsoft.com/dotnet/core/sdk:2.2-alpine as build-stage
COPY . /build
WORKDIR /build
RUN dotnet publish -c Release -o ../out

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-alpine
COPY --from=build-stage /build/out /app
WORKDIR /app
ENTRYPOINT ["dotnet", "SwitchLanNet.dll"]