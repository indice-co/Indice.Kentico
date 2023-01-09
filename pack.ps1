#!/usr/bin/env bash

# Clean and build in release
dotnet restore /nowarn:netsdk1138
dotnet clean

# Create all NuGet packages

dotnet pack src/Indice.Kentico.Oidc/Indice.Kentico.Oidc.csproj -c Release -p:KenticoVersion=9  -o ./artifacts
dotnet pack src/Indice.Kentico.Oidc/Indice.Kentico.Oidc.csproj -c Release -p:KenticoVersion=11 -o ./artifacts
dotnet pack src/Indice.Kentico.Oidc/Indice.Kentico.Oidc.csproj -c Release -p:KenticoVersion=12 -o ./artifacts