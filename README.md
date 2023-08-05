# BlazorWasmDeploy
# Blazor WASM Deploy
- Create
- Test (with mstest and bunit)
- Deploy

## Instructions
1. Create a new empty repo on GitHub
2. Create a new directory for your repo (should be the name of your project)
3. Navigate to that folder and open your project in VSCode
4. In VSCode, open a "bash" terminal and run the commands there.

### Create a Simple Static App and deploy it
Replace GITHUB_ACCOUNT_NAME with the name of your github account.
```bash
PROJECT_NAME=$(basename $PWD)
GITHUB_ACCOUNT_NAME="mariekauth"
git init
echo "<html><head><title>Test ${PROJECT_NAME}</title></head><body><h1>Deployed ${PROJECT_NAME}</h1></body></html>" >> index.html
git add .
git commit -m "Initial Commit"
git remote add origin "git@github.com:${GITHUB_ACCOUNT_NAME}/${PROJECT_NAME}.git"
git branch -M main
git push -u origin main
git checkout -b gh-pages
git push origin gh-pages
git checkout main
```

Your new site should now be active on github:
- You should be able to find the page by looking under "Action" -> Deployments, or under deployments on the right of your repo.

### Setup Your static Blazor App
The next script will:
- Remove the index page
- Add all the normal content, like .gitignore and License
- Add the Blazor App
- Add the build -> test -> deploy workflow
```bash
rm index.html
dotnet new gitignore
echo "# $PROJECT_NAME" >> README.md
echo ".vscode" >> .gitignore
cp ../LICENSE LICENSE
dotnet new sln -n ${PROJECT_NAME}
dotnet new blazorwasm -n "${PROJECT_NAME}" -o "${PROJECT_NAME}" -f net6.0
dotnet new mstest -n "${PROJECT_NAME}Test" -o "${PROJECT_NAME}Test" -f net6.0
dotnet sln "${PROJECT_NAME}.sln" add "${PROJECT_NAME}/${PROJECT_NAME}.csproj"
dotnet sln "${PROJECT_NAME}.sln" add "${PROJECT_NAME}Test/${PROJECT_NAME}Test.csproj"
dotnet add "${PROJECT_NAME}Test/${PROJECT_NAME}Test.csproj" package bunit -f net6.0
dotnet add "${PROJECT_NAME}Test/${PROJECT_NAME}Test.csproj" reference "${PROJECT_NAME}/${PROJECT_NAME}.csproj"
mkdir -p .github/workflows && touch .github/workflows/main.yml
```

# Update .github/workflows/main.yml and Deploy
Line 4, replace the value of PROJECT_NAME with the name of your project.
```yaml
name: Build -> Test -> Deploy
env: 
  # Set the Project Name Here
  PROJECT_NAME: BlazorWasmDeploy
on:
  push:
    branches:
    - main
permissions:
  contents: write

jobs:
  build:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        dotnet-version: [ '6.0.x' ]
    steps:
      - uses: actions/checkout@v3
      - name: Setup dotnet
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: ${{ matrix.dotnet-version }}

      - name: Display dotnet version
        run: dotnet --version

      - name: Clean
        run: dotnet clean

      - name: Install dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --configuration Release --no-restore

  test:
    needs: build
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v3
      - name: Setup dotnet
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: ${{ matrix.dotnet-version }}

      - name: Display dotnet version
        run: dotnet --version

      - name: Test
        run: dotnet test --logger trx --results-directory "TestResults-${{ matrix.dotnet-version }}"

  deploy-to-github-pages:
    needs: [build, test]
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET Core SDK
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 6.0.x

    - name: Install .NET WASM Build Tools
      run: dotnet workload install wasm-tools

    - name: Publish .NET Core Project
      run: dotnet publish ${PROJECT_NAME}/${PROJECT_NAME}.csproj -c:Release -p:GHPages=true -o dist/Web --nologo

    # changes the base-tag in index.html from '/' to 'ProjectName' to match GitHub Pages repository subdirectory
    - name: Change base-tag in index.html from / to ProjectName
      run: sed -i 's/<base href="\/" \/>/<base href="\/%PROJECT_NAME%\/" \/>/g' dist/Web/wwwroot/index.html

    # Replace %PROJECT_NAME% in index.html with the Variable set above to match GitHub Pages repository subdirectory
    - name: Change base-tag in index.html from / to ProjectName
      run: sed -i "s/%PROJECT_NAME%/$PROJECT_NAME/g" dist/Web/wwwroot/index.html

    # copy index.html to 404.html to serve the same file when a file is not found
    - name: copy index.html to 404.html
      run: cp dist/Web/wwwroot/index.html dist/Web/wwwroot/404.html

    # Add .nojekyll so _framework will get loaded
    - name: Add .nojekyll
      run: touch dist/Web/wwwroot/.nojekyll

    - name: Commit wwwroot to GitHub Pages
      uses: JamesIves/github-pages-deploy-action@3.7.1
      with:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        BRANCH: gh-pages
        FOLDER: dist/Web/wwwroot
```

Run the script below to deploy the update.

```bash
git add --all
git commit -m "Add Workflow CI/CD for github"
git push
```

This may take a moment to deploy.
After deployment the following should occur:
- Refreshing the hosted site https://{user}.github.io/{project}/ should display the new blazor app
- Anytime a change is pushed to the "main" branch the pipeline should run, and the changes should appear.

### Demo Testing
- Added Counter Test
- Added Count By 5
- Added Fibonacci Counter

I mentioned that without some of the modifications in the workflow file, there would be issues with connecting with the content and the routing links. These tests demonstrate that it does work.

The extra steps in the pipeline were necessary because we deploy to a subfolder (The Project Folder, instead of the root).

These steps may not be necessary when deploying to other cloud services (depending on the use case).

### Run Locally
From VSCode in Bash
```bash
dotnet test
dotnet run --project "${PROJECT_NAME}/${PROJECT_NAME}.csproj"
```

From Visual Studio
- Open the solution file
- Run the test normally with the Test Runner. (Tests -> Run All)
- F5 to run with debugging
- Ctrl+F5 to run without debugging

### TODO
- Some pages may return a 404 response, even though they return the appropriate content.
- Deploy to other environments
- Secure the app and add branch protections
