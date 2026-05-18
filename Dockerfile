# Stage 1: Build the React frontend
FROM node:20-alpine AS build-frontend
WORKDIR /app/frontend
COPY frontend/package*.json ./
RUN npm install
COPY frontend/ ./
ARG VITE_SUPABASE_URL
ENV VITE_SUPABASE_URL=$VITE_SUPABASE_URL
ARG VITE_SUPABASE_ANON_KEY
ENV VITE_SUPABASE_ANON_KEY=$VITE_SUPABASE_ANON_KEY
RUN npm run build

# Stage 2: Build the ASP.NET Core backend
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-backend
WORKDIR /app/backend
COPY backend/*.csproj ./
RUN dotnet restore
COPY backend/ ./
# Create wwwroot and copy frontend build
RUN mkdir -p wwwroot
COPY --from=build-frontend /app/frontend/dist ./wwwroot
RUN dotnet publish -c Release -o /app/publish

# Stage 3: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build-backend /app/publish .
# Copy init.sql so it's available when db.Database.ExecuteSqlRaw runs
COPY backend/init.sql .

# Set ASPNETCORE_URLS to bind to the Render-provided PORT (Render uses 10000 by default, but standard is binding to 0.0.0.0:$PORT or 8080)
# By default, .NET 8+ binds to 8080 which Render supports out-of-the-box, but we can respect the PORT env variable as well.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "backend.dll"]
