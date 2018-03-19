# BookRecommender

This is a documentation for bachelor thesis project Books Recommender Systems via Open Linked Data

## Getting Started

Acquire the files which should be as a electronic attachment to the thesis. You can find them at the online library repository of the dissertation, bachelor and masters thesis of MFF Charles University

### Running without SDK

For running the application, please run the BookRecommender file inside:
```
/builded/ubuntu_x64_16_04/BookRecommender
```
or
```
/builded/win10x64/BookRecommender.exe
```

### Running with SDK

Please download and install the latest SDK and runtime

SDK >= 1.04
Runtime >= 1.1

Then just call from the source file folder

```
dotnet run
```

After starting the application you can visit the address

```
localhost:5000
```


## If you want to extend the application by new SPARQL endpoint
See:
* /DataManipulation/SparqlEndpointMiner.cs
* /WikiData/WikiDataEndpointMiner.cs


## Authors

* **Ladislav Malecek** - *Initial work* - [PurpleBooth](https://github.com/LadislavMalecek)