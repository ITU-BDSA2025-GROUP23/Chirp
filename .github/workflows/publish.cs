name: .NET Release Chirp

on:
  push:
    tags:
      - "*" 

jobs:
  build-and-publish:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET 8
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.0.x

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore -c Release

      - name: Test
        run: dotnet test --filter "UnitTests|IntegrationTests" --no-build --verbosity normal

      - name: Build for Windows
        run: |
          tag=$(git describe --tags --abbrev=0)
          release_name="Chirp-$tag-win-x64"

          dotnet publish src/Chirp.Web/Chirp.Web.csproj --runtime win-x64 -c Release -o "$release_name" --self-contained false /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

          executable_path="./${release_name}/Chirp.Web.exe"
          7z a -tzip "${release_name}.zip" "$executable_path"

      - name: Build for Linux
        run: |
          tag=$(git describe --tags --abbrev=0)
          release_name="Chirp-$tag-linux-x64"

          dotnet publish src/Chirp.Web/Chirp.Web.csproj --runtime linux-x64 -c Release -o "$release_name" --self-contained false /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

          executable_path="./${release_name}/Chirp.Web"
          zip -r "${release_name}.zip" "$executable_path"

      - name: Build for Mac
        run: |
          tag=$(git describe --tags --abbrev=0)
          release_name="Chirp-$tag-osx-x64"

          dotnet publish src/Chirp.Web/Chirp.Web.csproj --runtime osx-x64 -c Release -o "$release_name" --self-contained false /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

          executable_path="./${release_name}/Chirp.Web"
          zip -r "${release_name}.zip" "$executable_path"

      - name: Create GitHub Release
        uses: softprops/action-gh-release@v1
        with:
          files: |
            Chirp-*.zip
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
