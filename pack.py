import os
from os.path import basename, abspath, split, isdir
from os.path import join as join_path
from zipfile import ZipFile

rootDir = "."

dirsToZip = []

if not isdir(abspath("./publish/zips")):
    os.mkdir(abspath("./publish/zips"))

"""
for dirName, subDirs, files in os.walk("./publish"):
    last_part = split(dirName)

    middle_part = list(split(last_part[0]))
    middle_part.append(last_part[1])

    first_part = list(split(middle_part[0])) + middle_part[1:]

    zipName = "-".join(first_part[1:])

    print(abspath(dirName) + " contains " +
          str(len(subDirs)) + " dirs and " + str(len(files)) + " files")
    print("")
#"""


#"""
for dirName, subDirs, files in os.walk("./publish"):
    if (len(subDirs) == 0 and not "zips" in dirName):

        last_part = split(dirName)

        middle_part = list(split(last_part[0]))
        middle_part.append(last_part[1])

        first_part = list(split(middle_part[0])) + middle_part[1:]

        zipName = "upject-v0.1__" + "-".join(first_part[1:])

        if ("publish-" in zipName):
            zipName = zipName.replace("publish-", "")

        dirsToZip.append(
            [
                zipName,
                abspath(dirName),
                list(map(lambda s: join_path(abspath(dirName), s), files))
            ]
        )


for dirName, dirPath, files in dirsToZip:
    zipFile = ZipFile(abspath("./publish/zips/" + dirName) + ".zip", 'w')
    print("Creating ./publish/zips/" + dirName + ".zip")
    for file in files:
        zipFile.write(file)
#"""