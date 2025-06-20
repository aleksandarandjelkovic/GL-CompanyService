FROM node:20-alpine AS build
WORKDIR /app

# Install dependencies first (this layer can be cached)
COPY package.json ./
RUN npm install

# Copy source code
COPY . .

# Build with production configuration
RUN NODE_OPTIONS=--max-old-space-size=4096 \
    npm run build -- \
    --configuration=production \
    --no-progress

# Runtime image
FROM nginx:alpine AS final
COPY --from=build /app/dist/company-client/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"] 