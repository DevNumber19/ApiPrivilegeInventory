FROM mcr.microsoft.com/dotnet/sdk:3.1 as builder
WORKDIR /src
ARG BUILD_ENV=dev
COPY . .
RUN dotnet publish --self-contained -r linux-x64 -c Release -o /app
COPY ./apiDealManagement/images /app/images
COPY ./apiDealManagement/download /app/download


FROM mcr.microsoft.com/dotnet/aspnet:3.1
MAINTAINER Titiwut M. <titiwut@feyverly.com>
ARG BUILD_ENV=dev
ENV TZ=Asia/Bangkok
EXPOSE 80
ENV DOTNET_URLS=http://0.0.0.0 \
    ASPNETCORE_URLS=http://0.0.0.0
WORKDIR /app
COPY --from=builder /app .
COPY .ci/setting/${BUILD_ENV}.json appsettings.json
ENTRYPOINT ["dotnet", "apiDealManagement.dll"]
