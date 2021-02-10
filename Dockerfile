FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build

# Install node
RUN curl -sL https://deb.nodesource.com/setup_14.x | bash
RUN apt-get update && apt-get install -y nodejs


WORKDIR /workspace
COPY . .
RUN dotnet tool restore

RUN dotnet fake build -t Bundle

# Copy complied app to alpine linux
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
COPY --from=build /workspace/deploy /app
WORKDIR /app
EXPOSE 8085

# Install Python
ENV PYTHONUNBUFFERED=1
RUN apk add --update --no-cache python3 && ln -sf python3 /usr/bin/python
RUN python3 -m ensurepip
RUN pip3 install --no-cache --upgrade pip setuptools

# Install Racket
# TODO

# Install bash on Alpine
RUN apk add --no-cache bash

# Copy demo strategies
WORKDIR /strategies
COPY strategies/ .

WORKDIR /app
# Run the server
ENTRYPOINT [ "dotnet", "Server.dll" ]



# RUN apk add openssh \
#      && echo "root:Dkuxsj22" | chpasswd
# EXPOSE 80 2222

# COPY sshd_config /etc/ssh/
# WORKDIR /scripts
# COPY startup.sh /scripts/startup.sh
# RUN ["chmod", "+x", "/scripts/startup.sh"]

# ENTRYPOINT ["/bin/bash", "/scripts/startup.sh"]