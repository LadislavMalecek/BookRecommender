# To build image run:
## dotnet publish
## docker build -t <image name> <path to project root>

# To run image:
## docker run -d -p 5000:5000 <image name>


FROM microsoft/aspnetcore

ENV INSIDE_DOCKER="yes"


# Copy database
COPY C:/netcore/SQLite/BookRecommender.db /app/db/

# COPY . /app

COPY bin/Debug/netcoreapp1.1/publish/ /app/
WORKDIR /app

# RUN ["dotnet", "restore"]
# RUN ["dotnet", "build"]

EXPOSE 5000

CMD ["dotnet", "BookRecommender.dll"]