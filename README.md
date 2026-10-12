# hff_revertbehavior - Revert something. 今は無きあの仕様をもう一度。

## 機能一覧 (全てGUIとコマンドの両方からアクセス可能)
### カメラ衝突の復活(ダークのゴール判定とか)(Jail Updateから)
### リスポーン高度の変更(10th Anniversary Updateから)
### ウォーター旧スポーンの復活(City Updateから?)

### コマンド追加
基本コマンド : 
```
revertbehavior <commands> <option>
```
または
```
rb <commands> [<option>]
```
### `commands`一覧(1行目はフルネームによる記述、2行目はエイリアスを使用した最短記法)
```
revertbehavior collidedefaulttrigger [<option>]
```
```
rb cdt [<option>]
```
カメラ衝突の復活
```
revertbehavior revertrespawnheight [<option>]
```
```
rb rrh [<option>]
```
リスポーン高度の変更
```
revertbehavior revertwaterspawn [<option>]
```
```
rb rws [<option>]
```
ウォーター旧スポーンの復活
`<option>`は全て設定する値が`bool`なので共通
- 引数なし : `True`/`False`を切り替え
- 正規攻略`^(t(rue)?|1|on)$`に一致する場合: `True`に設定
- 正規攻略`^(f(alse)?|0|off)$`に一致する場合: `false`に設定


## インストール
1. Steam版Human Fall Flatに`BepInEx`をインストール
2. [Releases](https://github.com/Msgames79/hff_revertbehavior/Releases/latest)からdllファイル(`hff_RevertBehavior-(version).dll`)をダウンロード
3. `hff_RevertBehavior-(version).dll`を`BepInEx\plugins`に置く

またはPowerShellで
```ps1
Invoke-RestMethod -Uri "https://raw.githubusercontent.com/Msgames79/hff_revertbehavior/refs/heads/main/install.ps1" | Invoke-Expression
```

## 過去の機能

## バージョン履歴
### 1.0.0
初回リリース