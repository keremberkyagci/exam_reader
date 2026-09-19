# 1. Base Image (Çalışma Ortamı)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# Tesseract ve Türkçe dil paketini yükle
RUN apt-get update && \
    apt-get install -y tesseract-ocr tesseract-ocr-tur libtesseract-dev && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# 2. Build Image (Derleme Ortamı)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ExamReader.Api.csproj", "./"]
RUN dotnet restore "./ExamReader.Api.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ExamReader.Api.csproj" -c Release -o /app/build

# 3. Publish (Yayınlama)
FROM build AS publish
RUN dotnet publish "ExamReader.Api.csproj" -c Release -o /app/publish

# 4. Final Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Tesseract'ın tessdata klasörünün nerede olduğunu belirt (Linux için genelde /usr/share/tesseract-ocr/4.00/tessdata şeklindedir)
# C# kodu içindeki _tessDataPath = "./tessdata" mantığını bozmamak adına Linux'un tessdata'sını uygulamanın içine kopyalıyoruz
RUN ln -s /usr/share/tesseract-ocr/5/tessdata /app/tessdata || \
    ln -s /usr/share/tesseract-ocr/4.00/tessdata /app/tessdata || true

ENTRYPOINT ["dotnet", "ExamReader.Api.dll"]
