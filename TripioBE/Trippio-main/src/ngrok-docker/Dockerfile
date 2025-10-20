# ---- Stage 1: lấy binary ngrok đã build sẵn (không đụng equinox) ----
FROM ngrok/ngrok:3 AS ngrok_src

# ---- Stage 2: final image siêu nhẹ dựa trên Alpine ----
FROM alpine:3.20

# Chỉ cần CA để TLS hoạt động (không cần wget/curl)
RUN apk add --no-cache ca-certificates

# Copy binary ngrok từ stage 1
COPY --from=ngrok_src /bin/ngrok /usr/local/bin/ngrok

# (Tùy chọn) Chạy non-root
RUN adduser -D -H -s /sbin/nologin ngrok && \
    mkdir -p /home/ngrok && chown -R ngrok:ngrok /home/ngrok
USER ngrok
WORKDIR /home/ngrok

# Thư mục cấu hình (v3 dùng ~/.config/ngrok/ngrok.yml)
RUN mkdir -p /home/ngrok/.config/ngrok

# Mở cổng web UI
EXPOSE 4040

# Healthcheck đơn giản (tùy chọn)
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD nc -z localhost 4040 || exit 0

# Entrypoint/CMD mặc định
ENTRYPOINT ["/usr/local/bin/ngrok"]
CMD ["--help"]
