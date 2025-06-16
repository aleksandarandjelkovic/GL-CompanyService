# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Install dependencies first (this layer can be cached)
COPY package.json ./
# Use npm install instead of npm ci to update lock file
RUN npm install

# Copy source code
COPY . .

# Build with production configuration
# Use more memory for the Node process
RUN NODE_OPTIONS=--max-old-space-size=4096 \
    npm run build -- \
    --configuration=production \
    --no-progress

# Runtime stage
FROM nginx:alpine
# Updated path for Angular 20 output
COPY --from=build /app/dist/company-client/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"] 