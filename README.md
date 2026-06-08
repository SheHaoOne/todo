# TodoApp — 仿 Microsoft To Do 的 WPF 桌面应用

一款完全离线运行的 WPF 待办事项应用，界面与交互仿照 Microsoft To Do，数据保存在本地，无需联网。

## 功能特性

### 智能列表
- **我的一天** — 聚焦今日任务
- **重要** — 已标记星标的任务
- **计划内** — 设置了截止日期的任务
- **全部** — 所有未完成任务
- **已完成** — 历史已完成任务

### 自定义列表
- 创建、删除自定义任务列表
- 每个列表独立管理任务

### 任务管理
- 添加、编辑、删除任务
- 圆形复选框标记完成/未完成
- 标记为重要（星标）
- 添加到「我的一天」
- 设置截止日期（今天 / 明天 / 下周）
- 子步骤（Steps）清单
- 备注（Notes）
- 已完成任务折叠/展开显示

### 数据存储
- 使用 JSON 文件本地持久化
- 存储路径：`%LocalAppData%\TodoApp\data.json`
- 无需数据库，无需网络

## 系统要求

- Windows 10 / 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## 构建与运行

```bash
# 克隆仓库
git clone <repo-url>
cd todo

# 还原依赖并构建
dotnet restore TodoApp.sln
dotnet build TodoApp.sln -c Release

# 运行
dotnet run --project TodoApp/TodoApp.csproj
```

也可以在 Visual Studio 2022 中打开 `TodoApp.sln`，按 F5 运行。

## 项目结构

```
TodoApp/
├── Models/          # 数据模型（任务、列表、步骤）
├── ViewModels/      # MVVM 视图模型
├── Services/        # 数据持久化与任务筛选
├── Converters/      # XAML 值转换器
├── Helpers/         # 命令辅助类
├── App.xaml         # 应用资源与样式
└── MainWindow.xaml  # 主界面
```

## 技术栈

- **.NET 8** + **WPF**
- **MVVM** 架构模式
- **CommunityToolkit.Mvvm**（可选扩展）
- **System.Text.Json** 本地数据持久化

## 截图说明

应用采用 Microsoft To Do 风格设计：
- 左侧导航栏：智能列表 + 自定义列表
- 中间任务区域：任务列表 + 添加任务输入框
- 右侧详情面板：任务标题、截止日期、步骤、备注

## 许可证

见 [LICENSE](LICENSE) 文件。
