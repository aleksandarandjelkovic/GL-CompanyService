export const environment = {
  production: false,
  // For local development outside Docker
  apiUrl: 'http://localhost:5000/api',
  authUrl: 'http://localhost:5001',
  
  // For Docker environment
  dockerApiUrl: '/api',
  dockerAuthUrl: '/auth'
}; 