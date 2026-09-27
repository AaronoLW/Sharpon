{
  description = "Sharpon";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
  };

  outputs = { self, nixpkgs }:
    let
      system = "x86_64-linux";
      pkgs = nixpkgs.legacyPackages.${system};
      dotnet-sdk = pkgs.dotnet-sdk_10;
      dotnet-runtime = pkgs.dotnetCorePackages.runtime_10_0;
    in {
      packages.${system}.default = pkgs.buildDotnetModule {
        pname = "sharpon";
        version = builtins.replaceStrings ["\n"] [""] (builtins.readFile ./VERSION);

        src = ../.;

        projectFile = "../Sharpon.csproj";

        dotnet-sdk = dotnet-sdk;
        dotnet-runtime = dotnet-runtime;

        nugetDeps = ./deps.nix;
      };

      devShells.${system}.default = pkgs.mkShell {
        packages = with pkgs; [
          git
          dotnet-sdk
        ];
      };
    };
}
