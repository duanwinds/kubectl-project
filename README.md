## 專案目的

比照 oc project 這個 sub command 的功能，用 dotnet 所開發的 kubectl plugin。

在 Mac/Linux 環境下，可以用 bash script 寫 kubectl plugin，但是在 windows 上，就要用 windows 能辨認的格式來開發，因此使用 dotnet 來開發這個 plugin。


## 使用流程如下:

* 下載 code
git clone https://github.com/duanwinds/kubectl-project.git

* 編譯 dotnet 程式
dotnet publish --configuration Release

* 編譯過程沒問題的話，把執行檔複製到 PATH 環境變數裡的路徑，例如 ~/bin/
cp bin/Release/net8.0/win-x64/publish/kubectl-project.exe ~/bin/kubectl-project.exe


## 測試 kubectl plugin 效果

* 顯示目前的設定的 project/namespace
$ kubectl project
  執行效果類似原生 kubectl 指令如下
$ kubectl config get-contexts $(kubectl config current-context) --no-headers | awk '{print $NF}'

* 設定到目標 namespace ，例如 kube-system
$ kubectl project kube-system
  執行效果類似原生 kubectl 指令如下
$ kubectl config set-context --current --namespace kube-system
