#! /bin/sh

# MIT License
# 
# Copyright (c) 2018 kevinta893
# 
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
# 
# The above copyright notice and this permission notice shall be included in all
# copies or substantial portions of the Software.
# 
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
# SOFTWARE.

PROJECT_PATH=$(pwd)/$UNITY_PROJECT_PATH
UNITY_BUILD_DIR=$(pwd)/build

project="SailingWithTheGods"

ERROR_CODE=0
echo "Items in project path ($PROJECT_PATH):"
ls "$PROJECT_PATH"

mkdir $UNITY_BUILD_DIR

############################

echo "Building project for Windows..."
/Applications/Unity/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -nographics \
  -silent-crashes \
  -logFile \
  -projectPath "$PROJECT_PATH" \
  -buildWindows64Player "$(pwd)/build/win/$project.exe" \
  -quit
  
if [ $? = 0 ] ; then
  echo "Building Windows exe completed successfully."
  ERROR_CODE=0
else
  echo "Building Windows exe failed. Exited with $?."
  ERROR_CODE=1
fi


###########################

echo "Building project for OSX..."
/Applications/Unity/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -nographics \
  -silent-crashes \
  -logFile \
  -projectPath "$PROJECT_PATH" \
  -buildOSXUniversalPlayer "$(pwd)/build/osx/$project.app" \
  -quit

if [ $? = 0 ] ; then
  echo "Building OSX app completed successfully."
  ERROR_CODE=0
else
  echo "Building OSX app failed. Exited with $?."
  ERROR_CODE=1
fi

###########################


echo 'Attempting to zip builds'
zip -r $(pwd)/Build/mac.zip $(pwd)/build/osx/
zip -r $(pwd)/Build/win.zip $(pwd)/build/win/



echo "Finishing with code $ERROR_CODE"
exit $ERROR_CODE
