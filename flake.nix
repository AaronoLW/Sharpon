{
  description = "My .NET project with devenv";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    devenv.url = "github:cachix/devenv";
    flake-parts.url = "github:hercules-ci/flake-parts";
  };

  outputs = inputs @ {
    self,
    nixpkgs,
    devenv,
    flake-parts,
    ...
  }:
    flake-parts.lib.mkFlake {inherit inputs;} {
      imports = [devenv.flakeModule];
      systems = ["x86_64-linux" "aarch64-linux" "aarch64-darwin" "x86_64-darwin"];

      perSystem = {
        config,
        self',
        inputs',
        pkgs,
        system,
        lib,
        ...
      }: {
        devenv.shells.default = {
          imports = [./devenv.nix];
        };

        packages = {
          default = self'.packages.sharpon;

          sharpon = pkgs.buildDotnetModule rec {
            nativeWayland = true;
            rid =
              {
                "x86_64-linux" = "linux-x64";
                "aarch64-linux" = "linux-arm64";
              }.${
                system
              } or (throw "Unsupported system: ${system}");

            pname = "sharpon";
            version = builtins.readFile ./VERSION;

            src = ./.;

            projectFile = "Sharpon.csproj";
            nugetDeps = ./deps.json;

            dotnet-sdk = pkgs.dotnetCorePackages.sdk_10_0;
            dotnet-runtime = pkgs.dotnetCorePackages.runtime_10_0;

            nativeBuildInputs = with pkgs; [
              copyDesktopItems
              makeWrapper

              clang
              llvmPackages.bintools
              zlib
              zstd
            ];

            selfContainedBuild = true;

            dotnetPublishFlags = [
              "-p:PublishAot=true"
              "-p:PublishSingleFile=true"
              "-p:StripSymbols=true"
              "-p:InvariantGlobalization=true" # Reduces size
              "-r:${rid}" # Runtime identifier
            ];

            runtimeDeps = with pkgs; [
              sdl3
              sdl3-image
              sdl3-ttf

              libglvnd

              udev

              iosevka
              roboto
            ];

            executables = ["Sharpon"];

            fixupPhase = ''
              runHook prefixup

              wrapProgram $out/bin/Sharpon \
                  ${lib.optionalString nativeWayland "--set SDL_VIDEODRIVER wayland"}

              ln -sft $out/lib/${pname} ${pkgs.sdl3}/lib/libSDL3${pkgs.stdenvNoCC.hostPlatform.extensions.sharedLibrary}

              runHook postFixup
            '';

            desktopItems = [
              (pkgs.makeDesktopItem {
                desktopName = "Sharpon";
                name = "Sharpon";
                exec = "Sharpon";
                icon = "Sharpon";
                comment = "Sharpon";
                type = "Application";
                categories = ["Development"];
              })
            ];

            meta = {
              description = "Der beste Code Editor der Welt!!!!!";
              homepage = "https://github.com/AaronoLW/Sharpon";
              license = lib.licenses.mit;
              platforms = ["x86_64-linux"];
              mainProgram = "Sharpon";
            };
          };
        };
      };
    };
}
