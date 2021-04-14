#!/bin/bash

build_for_target () {
    if [ -z $1 ]; then
        echo "Didn't specify any Target Runtime Moniker (TRM), exiting"
        exit 1
    fi

    target=$1

    dotnet restore

    # win64
    dotnet publish -f $target -r win-x64 --self-contained false -c Release -o publish/dependent/win64/$target
    dotnet publish -f $target -r win-x64 -c Release -o publish/standalone/win64/$target

    # osx64
    dotnet publish -f $target -r osx-x64 --self-contained false -c Release -o publish/dependent/osx64/$target
    dotnet publish -f $target -r osx-x64 -c Release -o publish/standalone/osx64/$target

    # linux64
    dotnet publish -f $target -r linux-x64 --self-contained false -c Release -o publish/dependent/linux64/$target
    dotnet publish -f $target -r linux-x64 -c Release -o publish/standalone/linux64/$target

    # cross-platform
    dotnet publish -f $target -c Release -o publish/cross-platform/$target

    # cleanup
    current_dir=`pwd`
    cd publish/cross-platform/$target
    find ./ -name 'upject' -o -name 'upject.exe' -o -name 'upject.pdb' -delete
    cd $current_dir
}

build_for_target "net5"
build_for_target "netcoreapp3.1"
build_for_target "netcoreapp3"

python pack.py