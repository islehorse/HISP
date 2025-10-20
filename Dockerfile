FROM mcr.microsoft.com/dotnet/sdk:9.0-noble AS build
WORKDIR /Build

# Copy everything
COPY . ./
# update and install python
RUN apt update && apt install python3 git -y

# Build and publish a release
WORKDIR /Build/HorseIsleServer
RUN dotnet publish -c Linux -p:PublishProfile=Linux64 HISPd
WORKDIR /Build

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:9.0-noble AS build-release-stage
COPY --from=build /Build/HorseIsleServer/HISPd/build/Linux-x64 /Build

WORKDIR /Build
ENTRYPOINT ["/Build/HISPd"]