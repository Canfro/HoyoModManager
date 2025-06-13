#!/bin/bash

# Verificación de argumentos
if [ "$#" -ne 2 ]; then
  echo "Uso: $0 <version> <mensaje>"
  exit 1
fi

VERSION="$1"
MESSAGE="$2"
BASE_DIR="HoyoModManager/bin/Release/net8.0"

# Construir lista de archivos comprimidos
ZIPS=()
for DIR in "$BASE_DIR"/*/publish; do
  if [ -d "$DIR" ]; then
    OS_NAME=$(basename "$(dirname "$DIR")")  # Extrae "linux-x64", "win-x64", etc.
    ZIP_NAME="HoyoModManager_${OS_NAME}.zip"

    echo "Comprimendo $DIR -> $ZIP_NAME"
    zip -j -r "$ZIP_NAME" "$DIR"/* >/dev/null

    ZIPS+=("$ZIP_NAME")
  fi
done

# Verificación final
if [ "${#ZIPS[@]}" -eq 0 ]; then
  echo "No se encontraron archivos para publicar."
  exit 1
fi

# Crear release con los .zip
gh release create "$VERSION" "${ZIPS[@]}" --title "$VERSION" --notes "$MESSAGE"

# Eliminar los .zip del disco
for ZIP in "${ZIPS[@]}"; do
  rm -f "$ZIP"
done
