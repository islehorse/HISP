FROM mcr.microsoft.com/dotnet/sdk:9.0-noble AS build
WORKDIR /Build

# Copy everything
COPY . ./
# update and install python
RUN apt update && apt install python3 git -y

# Build and publish a release
WORKDIR /Build/HorseIsleServer
RUN dotnet publish -c Linux -p:PublishProfile=Linux64 HISPd

#
# Build runtime image
#

FROM mcr.microsoft.com/dotnet/runtime:9.0-noble AS build-release
COPY --from=build /Build/HorseIsleServer/HISPd/build/Linux-x64 /usr/bin/hisp

ENV HISP_CONFIG_DIR /etc/hisp
ENV HISP_ASSETS_DIR /usr/bin/hisp
ENV HISP_LOG_FILE stdout

EXPOSE 12321
STOPSIGNAL sigstop

WORKDIR /usr/bin/hisp
ENTRYPOINT /usr/bin/hisp/HISPd