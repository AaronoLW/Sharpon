{
  pkgs,
  lib,
  config,
  inputs,
  ...
}: {
  languages.dotnet = {
    enable = true;
    package = pkgs.dotnetCorePackages.sdk_10_0;
  };

  packages = with pkgs; [
    nuget-to-json
  ];

  tasks = {
    "Sharpon:dependencies" = {
      exec = ''
        dotnet restore --packages out
        nuget-to-json out > deps.json
      '';
    };
    "Sharpon:build" = {
      after = ["Sharpon:dependencies"];
      exec = "exec dotnet publish";
    };
  };
}
