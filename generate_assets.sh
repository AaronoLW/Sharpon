#/usr/bin/env bash

set -e

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd)

echo \
"
internal static class AssetsGenerated
{
    public static readonly byte[] FontRubik = [$(xxd -p -c 32 "$SCRIPT_DIR/Assets/Rubik-Regular.ttf" | sed 's/../0x&, /g')];
    public static readonly byte[] FontRoboto = [$(xxd -p -c 32 "$SCRIPT_DIR/Assets/Roboto-Regular.ttf" | sed 's/../0x&, /g')];
}
" \
> $SCRIPT_DIR/AssetsGenerated.cs
