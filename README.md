sailingwiththegods
===================

[![License: LGPL v3](https://img.shields.io/badge/License-LGPL%20v3-blue.svg)](LICENSE) [![Codacy Badge](https://api.codacy.com/project/badge/Grade/ba9a42007847465d8bb80df93ad3dd77)](https://app.codacy.com/manual/kddressel/sailingwiththegods?utm_source=github.com&utm_medium=referral&utm_content=kddressel/sailingwiththegods&utm_campaign=Badge_Grade_Dashboard) ![CI](https://github.com/kddressel/sailingwiththegods/workflows/CI/badge.svg?branch=develop)

![](docs/images/screenshot.png)

This game is designed to:

* maximize immersion in the realities of sailing the ancient sea, closing the gap between our modern perspectives of sea travel and ancient realities
* generate data recorded from players’ behavioral patterns when making choices about how to move about the maritime networks
* integrate ancient voices from history and mythology with geospatial realities of economic resources, political trends and potential for gathering up to date knowledge

[Visit the website](https://scholarblogs.emory.edu/samothraciannetworks) for more information!

# Setup

This Build uses Unity 2019.4.9f1

While the main game code is open source, the game depends on a private repo for assets purchased from the Unity Asset Store. This should go in the ```Assets/_Proprietary``` folder. Access to the proprietary repo is limited, but if it is missing the project will populate with open source fallback assets upon loading for the first time in Unity.

## Setup Using SourceTree (recommended)
* Check if you have / need access to the proprietary repo
* Download and Install SourceTree
  * If asked, say you want embedded git, and you don't want mercurial
  * When asked if you want Bitbucket Server or Bitbucket, click "Skip"
  * So no when asked about loading an SSH key
* Add Github Account to SourceTree
  * Tools -> Options -> Authentication -> Add
  * Select Github for Hosting Service, leave everything else as it is (HTTPS, OAuth)
  * Click Refresh OAuth Token
  * Authenticate Github in the browser
* Clone the Repo
  * Make your own fork of the repository on the GitHub website
  * Click the Clone button
  * Copy paste the fork's HTTPS url into the URL field on the Clone screen
  * Choose manager-core on the popup that appear, and choose "Always use this"
  * If you get errors cloning the submodule, try going to Repository -> Respository Settings and double click your remote, and make sure Remote Account is your Github, not Generic
* Add Upstream Repo to your repository
  * Click Repository -> Repository Settings
  * Click Add
  * Type "upstream" as the remote name, and paste https://github.com/kddressel/sailingwiththegods.git in as the URL, change to your github account for remote account and click OK
  * Fetch -> And make sure you fetch all remotes (this is the default setting)
  * If needed, right click upstream/branchname and merge into your fork's branch

## Unzip the Navmesh

The Main Scene has a NavMesh that's over 100 MB so we have it committed to git as a ZIP file and the actual NavMesh asset is gitignored. After pulling for the first time, you need to unzip. You should see a "Zipped assets have been updated, extract them?" popup. Choose "Yes (recommended)" to automatically extract the zip file.

If you do not see this popup or need to manually unzip, you can either use the SWTG -> Unzip Assets menu, or you can unzip the file manually:

```Assets/_Scenes/Main Scene/NavMesh.zip```

## Git Setup for Windows

* Install [Git for Windows](https://git-scm.com/download/win) using **default settings**
  * Make sure you pick **"Git Credential Manager Core"**
* Check if you have / need access to the proprietary repo
* Use https athentication steps below to clone your fork

## Clone the project using https authentication

```
git clone yourforkurl --branch develop
git remote add upstream https://github.com/kddressel/sailingwiththegods.git

# and if you have access to the proprietary repo...
git submodule update --init
```

* After running git submodule update --init, you should get a github login popup
* Choose "Sign in with your browser"
* Authorize GitCredentialManager to your github account
* Wait for a while. The submodule is large and may not appear to be downloading immediately

Most of the team is using https, but if you would like to use SSH authentication, follow [this guide](docs/ssh-auth.md).


# Documentation

* [Coding Conventions](docs/coding-convention.md)
* [UI System](docs/ui-system.md)
