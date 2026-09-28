# Nocturne Skill Evolution

『真・女神転生III NOCTURNE HD REMASTER』Steam版向けのMelonLoader MODです。

Skill Power-Up / Skill Mutationが発生したとき、元のスキルを残したまま、変化後のスキルを追加で習得できるようにします。Skill Power-Up / Mutationの発生率や、Power-Upの繰り返し制限も調整できます。

現在のバージョンは **0.1.0**（初回公開）です。旧名「Nocturne Add New Skills」から名称を変更しました（[旧版からの更新](#旧版nocturne-add-new-skillsからの更新)）。

> **English summary**
>
> Nocturne Skill Evolution is a MelonLoader mod for Shin Megami Tensei III: Nocturne HD Remaster (Steam).
>
> When Skill Power-Up or Skill Mutation occurs, the demon can keep the original skill and learn the transformed skill as an additional skill. When all 8 skill slots are full, the game's own forget-selection screen is used.
>
> It also provides configurable Power-Up / Mutation chances (0% / Native / 100%), Power-Up repeat behavior (Native / Unlimited), a global ON/OFF toggle, and an optional in-game settings GUI through Nocturne Modern Controller v3.0.0 (Japanese / English, following the Controller language). The mod works standalone; Controller is not required.
>
> **Install:** extract `NocturneSkillEvolution-v0.1.0.zip` into the game folder (`smt3hd`). Requirements: Steam version, Windows x64, MelonLoader 0.6.x (tested with v0.6.1 Open-Beta).
>
> **Updating from "Nocturne Add New Skills":** delete `Mods/NocturneAddNewSkills.dll` (do not keep both DLLs). Existing users of the old Nocturne Add New Skills build can keep their settings: if `NocturneSkillEvolution.settings.json` does not exist, the mod automatically imports `NocturneAddNewSkills.settings.json` on first start. The old settings file is not deleted.

## 主な機能

- **元のスキルを保持**: Skill Power-Up / Skill Mutation が起きても、元のスキルは上書きされずに残ります。
- **変化後スキルを追加で習得**: 強化・変化後のスキルを、元のスキルとは別に追加で習得します。
- **8枠満杯時はゲーム本来の忘却画面**: スキルが8つ埋まっている場合は、ゲーム標準の「忘れるスキルを選ぶ」画面で、忘れるスキルを1つ選びます。
- **Skill Power-Up の発生率**: 0% / 通常 / 100% から選択。
- **Skill Mutation の発生率**: 0% / 通常 / 100% から選択。
- **Skill Power-Up の繰り返し**: 通常 / 無制限 から選択。
- **MOD全体のON/OFF**: OFFにするとゲーム本来の挙動に戻ります。各設定値はそのまま保持されます。
- **Nocturne Modern Controller v3.0.0 との連携（任意）**: 導入している場合、Controllerの設定GUI「MOD機能」タブから設定できます。表示はControllerの言語設定に合わせて日本語 / Englishに切り替わります。

## 必要環境

- Steam版 SMT3 Nocturne HD Remaster（Windows x64）
- MelonLoader 0.6.x（動作確認: v0.6.1 Open-Beta）

Nocturne Modern Controller は**必須ではありません**。このMODは単体で動作します。

## インストール

1. `NocturneSkillEvolution-v0.1.0.zip` をゲームフォルダー（`smt3hd`）へそのまま展開します。ZIPの中は`Mods/`から始まる構成です。

   ```text
   smt3hd/
     Mods/
       NocturneSkillEvolution.dll
   ```

2. ゲームを起動します。初回起動時に設定ファイル`Mods/NocturneSkillEvolution.settings.json`が作られます（ZIPには含まれません）。Controller連携用の`Mods/NocturneModernSkillEvolution.features.json`も自動で作られます。

## 旧版（Nocturne Add New Skills）からの更新

このMODは、以前「Nocturne Add New Skills」（`NocturneAddNewSkills.dll`）という名前で公開していたものと同じMODです。

1. `Mods/NocturneAddNewSkills.dll` を**削除**してください。旧DLLと新DLLを両方置くと、同じ処理が二重に動きます。
2. 新しい`NocturneSkillEvolution-v0.1.0.zip`を展開します。
3. 設定は引き継がれます。`NocturneSkillEvolution.settings.json`がまだ無い場合、初回起動時に旧設定`NocturneAddNewSkills.settings.json`の内容を自動で取り込みます。旧設定ファイルは削除されずに残ります（不要なら手動で削除できます）。
4. Controller連携用の旧ファイル`NocturneModernAddNewSkills.features.json`は、新版の初回起動時に自動で削除されます（Controllerの画面にカードが二重に表示されるのを防ぐため）。

## 設定

| 項目 | 設定ファイルのキー | 値 | 既定 |
|---|---|---|---|
| 変化後スキルを追加習得（MOD全体のON/OFF） | `Enabled` | `true` / `false` | `true` |
| スキル変化：発生率 | `SkillMutation.Chance` | `Disabled`（0%） / `Native`（通常） / `Always`（100%） | `Native` |
| スキル強化：発生率 | `SkillPowerUp.Chance` | `Disabled`（0%） / `Native`（通常） / `Always`（100%） | `Native` |
| スキル強化：繰り返し | `SkillPowerUp.Repeat` | `Native`（通常） / `Unlimited`（無制限） | `Native` |

設定ファイルの例（既定値）:

```json
{
  "Enabled": true,
  "SkillMutation": {
    "Chance": "Native"
  },
  "SkillPowerUp": {
    "Chance": "Native",
    "Repeat": "Native"
  }
}
```

設定ファイルには、表示言語に関係なく常に上の内部値（`Native`、`Unlimited`など）が保存されます。

### 各項目の意味

- **変化後スキルを追加習得**: MOD全体のON/OFFです。OFFにすると、変化後スキルの追加習得・発生率の変更・繰り返しの変更がすべて止まり、ゲーム本来の挙動になります。発生率・繰り返しの設定値は変更されずに保持され、ONに戻すとそのまま使われます。
- **スキル変化：発生率**
  - 0%: スキル変化を起こしません。
  - 通常: ゲーム本来の確率です。
  - 100%: 変化できるスキル候補がある場合、必ず変化を試みます（成立するかどうかはゲーム本来の判定に従います）。
- **スキル強化：発生率**
  - 0%: スキル強化を起こしません。
  - 通常: ゲーム本来の確率です。
  - 100%: レベルアップ時のスキル変化判定1回につき最大1回、スキルの変化を保証します（1回の戦闘で複数レベル上がった場合も、ゲーム本来と同じく判定は1回です）。強化できるスキルがあればSkill Power-Upを、強化できるスキルが無い場合は変化できるスキルがあればSkill Mutationを起こします。
- **スキル強化：繰り返し**
  - 通常: ゲーム本来の挙動です（一度強化したあとは、同じ仲魔で続けて強化が起きにくくなる制限があります）。
  - 無制限: その制限による不成立だけを解除し、続けて強化が起きるようにします。

### 設定の変更方法

- **Nocturne Modern Controller v3.0.0 を導入している場合**: 設定GUIの「MOD機能」タブに「Nocturne Skill Evolution」のカードが表示され、ゲーム中に変更できます。変更は設定ファイルにも保存されます。表示はControllerの言語設定（日本語 / English）に合わせて切り替わります。Controllerの言語を変更した場合は、設定GUIを開き直すと反映されます。
- **Controllerを導入していない場合**: `Mods/NocturneSkillEvolution.settings.json` をテキストエディタで編集し、ゲームを再起動してください。不正な値は`Native`（通常）として扱われます。

## Nocturne Modern Controller との連携

このMODはController DLLを参照せず、単体で動作します。Nocturne Modern Controller v3.0.0以降が導入されている場合のみ、`Mods`フォルダー内の機能情報ファイル（`NocturneModernSkillEvolution.features.json`、自動生成）を通じて、設定GUIの「MOD機能」タブから設定できるようになります。

| 表示（日本語） | 表示（English） | 値 |
|---|---|---|
| 変化後スキルを追加習得 | Learn Transformed Skills | ON / OFF |
| スキル変化：発生率 | Skill Mutation: Chance | 0% / 通常 / 100%（0% / Native / 100%） |
| スキル強化：発生率 | Skill Power-Up: Chance | 0% / 通常 / 100%（0% / Native / 100%） |
| スキル強化：繰り返し | Skill Power-Up: Repeat | 通常 / 無制限（Native / Unlimited） |

- Controller: https://github.com/tanatyucom/NocturneModernController

## アンインストール / 無効化

- **一時的に無効化**: 設定の「変化後スキルを追加習得」をOFFにします（設定ファイルでは`"Enabled": false`）。
- **アンインストール**: `Mods`フォルダーから次のファイルを削除します。
  - `NocturneSkillEvolution.dll`
  - `NocturneSkillEvolution.settings.json`（設定を残さない場合）
  - `NocturneModernSkillEvolution.features.json`（Controller連携用の自動生成ファイル）
  - 旧版から更新した場合は、`NocturneAddNewSkills.settings.json`が残っていることがあります（自動では削除しません。不要なら削除してください）。

MODで習得したスキルは、アンインストール後もセーブデータに残ります。

## 注意事項・既知の制限

- 導入前・設定変更前にセーブデータのバックアップを推奨します。
- MODがONの間に「元のスキル」と「強化後のスキル」を両方習得した仲魔は、MODをOFFにした状態でゲーム本来のSkill Power-Upが起きると、同じスキルを重複して持つ可能性があります（現時点で実際の発生は確認されていません）。

## ソースコード / ライセンス

- Source: https://github.com/tanatyucom/NocturneSkillEvolution
- License: [MIT License](LICENSE)

## クレジット

- Author: Gray Ghost（GitHub: tanatyucom）
- [MelonLoader](https://github.com/LavaGang/MelonLoader) / [HarmonyLib](https://github.com/pardeike/Harmony) を利用しています（配布物には含まれません）。

## 開発者向け情報

### ビルド

net6ベースのMelonLoader / Il2CppInteropアセンブリを使用します。MelonLoaderを導入したSMT3HDが生成したIl2Cppアセンブリが必要です。既定の`GameDir`はSteam標準配置（`C:\Program Files (x86)\Steam\steamapps\common\smt3hd`）を参照します。

```powershell
dotnet build -c Release
# 別の場所にゲームがある場合
dotnet build -c Release -p:GameDir="D:\SteamLibrary\steamapps\common\smt3hd"
```

### 調査記録

ネイティブ解析（GameAssembly.dll / global-metadata.dat）と設計検討の記録は`docs/`と`investigations/`にあります。

- [`docs/investigation-log.md`](docs/investigation-log.md)
- [`docs/ROADMAP.md`](docs/ROADMAP.md)

本プロジェクトは`NocturneModernGameplay`、`NocturneAddNewSkills`（公開名「Nocturne Add New Skills」）という旧名で開発・公開されていました。調査記録内の旧名は当時の記録としてそのまま残しています。
