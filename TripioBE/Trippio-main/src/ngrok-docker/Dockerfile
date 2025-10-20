# Multi-stage build để tạo ngrok image nhẹ
FROM alpine:latest

# Cài đặt dependencies cần thiết
RUN apk add --no-cache ca-certificates wget unzip

# Download ngrok binary cho Linux
RUN wget https://bin.equinox.io/c/bNyj1mQVY4c/ngrok-v3-stable-linux-amd64.tgz && \
    tar -xzf ngrok-v3-stable-linux-amd64.tgz && \
    mv ngrok /usr/local/bin/ngrok && \
    chmod +x /usr/local/bin/ngrok && \
    rm ngrok-v3-stable-linux-amd64.tgz

# Create ngrok config directory
RUN mkdir -p /root/.ngrok2

# Set working directory
WORKDIR /app

# Expose ngrok web interface port
EXPOSE 4040

# Entry point
ENTRYPOINT ["/usr/local/bin/ngrok"]
CMD ["--help"]
