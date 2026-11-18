FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

RUN apt-get update && apt-get install -y \
    clang zlib1g-dev cmake build-essential git make \
    pkg-config ninja-build gnome-desktop-testing libasound2-dev libpulse-dev \
    libaudio-dev libfribidi-dev libjack-dev libsndio-dev libx11-dev libxext-dev \
    libxrandr-dev libxcursor-dev libxfixes-dev libxi-dev libxss-dev libxtst-dev \
    libxkbcommon-dev libdrm-dev libgbm-dev libgl1-mesa-dev libgles2-mesa-dev \
    libegl1-mesa-dev libdbus-1-dev libibus-1.0-dev libudev-dev libthai-dev libusb-1.0-0-dev \
    libpipewire-0.3-dev libwayland-dev libdecor-0-dev liburing-dev \
    libharfbuzz-dev libfreetype6-dev libpng-dev \
    && rm -rf /var/lib/apt/lists/*

COPY . .

WORKDIR /SDL

RUN git clone --depth 1 --branch release-3.4.x https://github.com/libsdl-org/SDL.git SDL \
    && cmake -S SDL/ -B SDL/build \
        -DCMAKE_BUILD_TYPE=Release \
        -DSDL_STATIC=ON \
        -DSDL_SHARED=OFF \
        -DSDL_TESTS=OFF \
        -DSDL_MAIN_HANDLED=ON \
    && cmake --build SDL/build -j$(nproc) \
    && cmake --install SDL/build \
    && cp SDL/build/libSDL3.a /src/
    
RUN git clone --depth 1 --branch release-3.4.x https://github.com/libsdl-org/SDL_image.git SDL_image \
    && cmake -S SDL_image/ -B SDL_image/build \
        -DCMAKE_BUILD_TYPE=Release \
        -DBUILD_SHARED_LIBS=OFF \
        -DSDLIMAGE_VENDORED=OFF \
        -DSDLIMAGE_SAMPLES=OFF \
        -DSDLIMAGE_TESTS=OFF \
        -DSDLIMAGE_INSTALL=ON \
        -DSDLIMAGE_DEPS_SHARED=OFF \
    && cmake --build SDL_image/build -j$(nproc) \
    && cp SDL_image/build/libSDL3_image.a /src/
    
RUN git clone --depth 1 --branch release-3.2.x https://github.com/libsdl-org/SDL_ttf.git SDL_ttf \
    && cmake -S SDL_ttf/ -B SDL_ttf/build \
        -DCMAKE_BUILD_TYPE=Release \
        -DBUILD_SHARED_LIBS=OFF \
        -DSDLTTF_VENDORED=OFF \
        -DSDLTTF_SAMPLES=OFF \
        -DSDLTTF_INSTALL=ON \
        -DSDLTTF_HARFBUZZ=ON \
        -DSDLTTF_PLUTOSVG=OFF \
        -DCMAKE_PREFIX_PATH=/opt/sdl3 \
        -DCMAKE_INSTALL_PREFIX=/opt/sdl3 \
    && cmake --build SDL_ttf/build -j"$(nproc)" \
    && cp SDL_ttf/build/libSDL3_ttf.a /src/
    
WORKDIR /src
    
RUN dotnet publish -c Release -r linux-x64 -o /app/publish-portable/ -p:Portable=true

RUN rm -f /app/publish-portable/*.so

FROM scratch AS export

COPY --from=build /app/publish-portable/ /
