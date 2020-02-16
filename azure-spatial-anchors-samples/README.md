# Azure Spatial Anchors サンプルコード

このサンプルコードは Azure Spatial Anchorsのサンプルコード[https://github.com/Azure/azure-spatial-anchors-samples](https://github.com/Azure/azure-spatial-anchors-samples)をベースにMixed Reality Toolkit V2を利用して
HoloLens,iOs,Androidの3デバイスのマルチプラットフォーム開発を行うサンプルです。

## 開発環境
このサンプルは以下の環境のもとで作成しました。

### Windows環境
* Windows 10 Pro 1903
* Unity 2019.1.10f1
    * iOS Build Support
        * Android SDK & NDK tools 
    * Android Build Support
    * Universal Windows Platform Build Support
* Visual Studio 2019 
    * C++ によるデスクトップ開発
    * ユニバーサル Windows プラットフォーム (UWP) の開発 
* Windows SDK
    * Windows 10 SDK (10.0.17763.0) ※HoloLens 1
    * Windows 10 SDK (10.0.18362.0) ※HoloLens 2

### Mac開発環境
* MacOS Catalina 10.15.2
* Xcode 11.3(11C29) 

使用しているモジュールとそのバージョンは以下の通りです。

|製品名|入手先|バージョン|
|:-:|:-|:-:|
|Azure Spatial Anchors SDK|[git - tags V2.1.1](https://github.com/Azure/azure-spatial-anchors-samples/releases/tag/v2.1.1)<br/>[AzureSpatialAnchors.unitypackage](https://github.com/Azure/azure-spatial-anchors-samples/releases/download/v2.1.1/AzureSpatialAnchors.unitypackage)|V2.1.1|
|Mixed Reality Toolkit - Foundation |[git - tags V2.2.0](https://github.com/microsoft/MixedRealityToolkit-Unity/releases/tag/v2.2.0)<br/>[Microsoft.MixedReality.Toolkit.Unity.Foundation.2.2.0.unitypackage](https://github.com/microsoft/MixedRealityToolkit-Unity/releases/download/v2.2.0/Microsoft.MixedReality.Toolkit.Unity.Foundation.2.2.0.unitypackage)|V2.2.0|
|Mixed Reality Toolkit - UnityAR |[git - tags V2.2.0](https://github.com/microsoft/MixedRealityToolkit-Unity/releases/tag/v2.2.0)<br/>[Microsoft.MixedReality.Toolkit.Unity.Providers.UnityAR.2.2.0.unitypackage](https://github.com/microsoft/MixedRealityToolkit-Unity/releases/download/v2.2.0/Microsoft.MixedReality.Toolkit.Unity.Providers.UnityAR.2.2.0.unitypackage)|V2.2.0|
|AR Foundation|Unity Package Manager経由|2.1.4|
|ARCore XR Plugin|Unity Package Manager経由|2.2.0-preview2|
|ARKit XR Plugin|Unity Package Manager経由|2.1.2|

## サンプルの実行手順

Mixed Reality Toolkit V2.2.0はOSの長いパスの問題によってプロジェクトが正しく動作しない場合があります。できるだけルートに近いパス（D:\AzureSpatialAnchorsSamplesOnMRTKV2\azure-spatial-anchors-samples)で作業をしてください。

### Azureの準備
1. [docs.microsoft.comのAzure Apatial Anchorsのドキュメント「Spatial Anchors リソースを作成する」](https://docs.microsoft.com/ja-jp/azure/spatial-anchors/tutorials/tutorial-share-anchors-across-devices?tabs=VS%2CAndroid#create-a-spatial-anchors-resource)を参考にAzureポータルでAzure Spatial Anchorsのサービスを追加する。
2. [docs.microsoft.comのAzure Apatial Anchorsのドキュメント「アンカー共有サービスのデプロイ」](https://docs.microsoft.com/ja-jp/azure/spatial-anchors/tutorials/tutorial-share-anchors-across-devices?tabs=VS%2CAndroid#deploy-your-sharing-anchors-service)を参考にAnchor共有用のWebサービスを作成する　

### プロジェクトの設定
1. このリポジトリをCloneもしくはZipでダウンロードする
2. [AzureSpatialAnchorsSamplesOnMRTKV2/azure-spatial-anchors-samples](https://github.com/TakahiroMiyaura/AzureSpatialAnchorsSamplesOnMRTKV2/tree/master/azure-spatial-anchors-samples)をUnityで開く
3. [Microsoft.MixedReality.Toolkit.Unity.Foundation.2.2.0.unitypackage]をインポート
4. [Microsoft.MixedReality.Toolkit.Unity.Providers.UnityAR.2.2.0.unitypackage]をインポート
5. [Project]タブから[Assets/MixedRealityToolkit.Staging/UnityAR/Microsoft.MixedReality.Toolkit.Providers.UnityAR]を選択する
6. [Inspector]タブを選択し、[Assembly Definition References]に[UnityEngine.SpatialTracking]の参照を追加し[Apply]を押下
7. [[unitypackage/Microsoft.MixedReality.Toolkit.Extensionkit.0.9.0.unitypackage]](https://github.com/TakahiroMiyaura/AzureSpatialAnchorsSamplesOnMRTKV2/tree/master/unitypackages/Microsoft.MixedReality.Toolkit.Extensionkit.0.9.0.unitypackage)をインポート 
8. [[unitypackage/Microsoft.MixedReality.Toolkit.Providers.UnityAR.AzureSpatialAnchors.0.9.0.unitypackage]](https://github.com/TakahiroMiyaura/AzureSpatialAnchorsSamplesOnMRTKV2/tree/master/unitypackages/Microsoft.MixedReality.Toolkit.Providers.UnityAR.AzureSpatialAnchors.0.9.0.unitypackage)をインポート
9. TMP Essencialのインポートを行います。
    1. [Project]タブから[Assets/AzureSpatialAnchors.Examples/Scenes/AzureSpatialAnchorsBasicDemo.unity]を選択しシーンを開く
    2. シーンを開いたあと、[TMP Importer]ウインドウが表示されるので[Import TMP Essentials]を押してインポートを行う。  
10. [手順1.]で作成したSpatial Anchorsリソースの[アカウントID], [アカウントKey]を設定する。
    1.  [Project]タブから[Assets/AzureSpatialAnchors.SDK/Resouces/SpatialAnchorConfig]を選択
    2.  [Inspector]タブからパラメータを設定する
        * [Autorication Mode]を[APIKey]に設定する
        * [Spatial Anchors Account Id]に[手順1.]で作成したサービスの[アカウントID]を設定する
        * [Spatial Anchors Account Key]に[手順1.]で作成したサービスの[アカウントKey]を設定する
11. [手順2.]で作成したWebサービスのパスを設定する。
    1.  [Project]タブから[Assets/AzureSpatialAnchors.Examples/Resouces/SpatialAnchorSamplesConfig]を選択
        * [Base Sharing URL]に[手順2.]で作成したAppサービスの[URL]を設定する

### アプリケーションのデプロイ
#### Androidの場合  

1. メニュー[File]-[Build Settings]を選択し、[Build Setting]ウィンドウを表示する
2. [Platform]を[Android]に変更し、[Switch Platform]を押す
3. メニュー[Mixed Reality Toolkit]-[Utilties]-[Configure Unity Project]を選択し、[MRTK Project Configurator]ウィンドウを表示する
4. [[MRTK Project Configurator]ウィンドウ]内の[Apply]ボタンを押す
5. メニュー[Edit]-[Project Settings...]を選択し、[Project Settings]ウィンドウを開き、[Player]を選択する
6. [Product Name]を[任意の名前]に変更する。
7. [Android]タブを選択し、[Other Settings]を展開する
8. [Color Space]を[Gamma]に設定する。
9. [Auto Graphics API]のチェックを外す
10. [Graphics APIs]を[OpenGLESS3]のみにする
11. [Package Name]を[任意の名前空間+[Product Name]]に変更する
12. [Minimum API Level]を[Android 8.0 'Oreo' (API Level 26)]に変更する。
13. [Android]タブを選択し、[Publish Settings]を展開する
14. [Custom Gradle Template]にチェックを入れ、[パス]が[Assets/Plugins/Android/mainTemplate.gradle]であることを確認する
15. [File]-[Build Settings]を選択し、[Build Setting]ウィンドウを表示する
16. [Build]を実行し、生成されたAPKをAndroid端末にインストールする
#### iOSの場合

1. メニュー[File]-[Build Settings]を選択し、[Build Setting]ウィンドウを表示する
2. [Platform]を[iOS]に変更し、[Switch Platform]を押す
3. メニュー[Mixed Reality Toolkit]-[Utilties]-[Configure Unity Project]を選択し、[MRTK Project Configurator]ウィンドウを表示する
4. [[MRTK Project Configurator]ウィンドウ]内の[Apply]ボタンを押す
5. メニュー[Edit]-[Project Settings...]を選択し、[Project Settings]ウィンドウを開き、[Player]を選択する
6. [Product Name]を[任意の名前]に変更する。
7. [iOS]タブを選択し、[Other Settings]を展開する
8. [Color Space]を[Gamma]に設定する。
9.  [Auto Graphics API]のチェックを外す
10. [Graphics APIs]を[sRGB]のみにする
11. [Bundle Identifier]を[任意の名前空間+[Product Name]]に変更する
12. [Camera Usage Description]に任意の文字列を入力する
13. [Location Usage Description]に任意の文字列を入力する
14. [Architecture]を[ARM64]に変更する
15. [File]-[Build Settings]を選択し、[Build Setting]ウィンドウを表示する
16. [Build]を実行し、xCode用のコードを出力するフォルダを指定する
17. 生成したフォルダをXcodeをMac環境にコピーする
18. Mac環境でコンソールを開きコピーしたフォルダ配下で以下のコマンドを実行する

```
$cd [コピー先のパス]
$pod install --repo-update
$open ./Unity-iPhone.xcworkspace
```

19. Xcodeが開くのでデバイスにデプロイする

#### UWP（HoloLens）の場合
1. メニュー[File]-[Build Settings]を選択し、[Build Setting]ウィンドウを表示する
2. [Platform]を[Universal Windows Platform]に変更し[Switch Platform]を押す
3. メニュー[Mixed Reality Toolkit]-[Utilties]-[Configure Unity Project]を選択し、[MRTK Project Configurator]ウィンドウを表示する
4. [[MRTK Project Configurator]ウィンドウ]内の[Apply]ボタンを押す
5. メニュー[Edit]-[Project Settings...]を選択し、[Project Settings]ウィンドウを開き、[Player]を選択する
6. [Product Name]を[任意の名前]に変更する。
7. [Universal Windows Platform]タブを選択し、[Publish Settings]を展開する
8. [Package Name]を[任意の名前]に変更する。
9. [Capabilities]内の以下の項目チェックを入れる
    * InternetClient
    * InternetClientServer
    * WebCam
    * Proxymitye
    * Location
    * Bluetooth
    * SpatialPerception
10.  [XR Settings]を展開し[Virtual Reality SDKs]に[Windows Mixed Reality]を追加されていることを確認する
11.  [Build]を実行し、UWP用のコードを出力フォルダを指定する
12.  出力フォルダの中にあるソリューションファイルをVisual Studioで開く
13.  デプロイ先に応じて[Architecture]を選択する
    * [HoloLens 1]の場合、[x86]
    * エミュレータの場合、[x86]
    * [HoloLens 2]の場合、[Arm],[Arm64]
14. [構成]を選択する
    *  [Debug] デバッグで動作確認を行う場合に設定
    *  [Release] パフォーマンスなどの動作検証を行う場合に設定
    *  [Master] ストアアプリに公開する場合に設定
15. 実機にデプロイする
