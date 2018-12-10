# To build image run:
## dotnet publish
## docker build -t <image name> <path to project root>

# To run image:
## docker run -d -p 5000:5000 <image name>


FROM microsoft/dotnet:2.1-sdk

# Copy database
COPY BookRecommender.db /app/

# COPY . /app
COPY . /app/
WORKDIR /app

RUN ["dotnet", "restore"]
RUN ["dotnet", "build"]
CMD ["dotnet", "run"]