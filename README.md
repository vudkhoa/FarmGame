# FarmGame
## Mục Lục

- [FarmGame](#farmgame)
  - [Contributors](#contributors)
  - [GameManager](#gamemanager)
  - [Design Pattern](#design-pattern)
    - [1) Mô Hình MVC](#1-mô-hình-mvc)
    - [2) Singleton cho Manager, Controller](#2-singleton-cho-manager-controller)
    - [3) State cho WorkerModel](#3-state-cho-workermodel)
  - [UI MANAGER](#ui-manager)
  - [UI CANVAS](#ui-canvas)
  - [CsvConfigLoader.cs](#csvconfigloadercs)
  - [Mô phỏng hoạt động sản xuất/thu hoạch của worker](#mô-phỏng-hoạt-động-sản-xuấtthu-hoạch-của-worker)
  - [Hàm Quan Trọng](#hàm-quan-trọng)
    - [OnlGame_CheckWorking](#onlgame_checkworking)
    - [OffGame_CheckWorking](#offgame_checkworking)
    - [GetPlotListFromOldData (Trong PlotController.cs)](#getplotlistfromolddata-trong-plotcontrollercs)
    - [FillToPlot (Trong WorkerController.cs)](#filltoplot-trong-workercontrollercs)
  - [Link Build](#link-build)


## Contributors
<p align="left">
  <a href="https://github.com/vudkhoa">
    <img src="https://github.com/vudkhoa.png" width="80" height="80" alt="vudkhoa" style="border-radius:50%;">
  </a>
</p>

## GameManager

`GameManager` là **điểm vào** của gameplay: giữ **trạng thái game**, khởi tạo **các controller** cốt lõi và điều phối LoadSave Data.

---

## Vai trò chính
- **Bootstrap hệ thống**: tạo các module runtime từ **Resources**: `DataManager`, `PlayerController`, `WorkerController`,..., hạn chế để tràn lan trên scene, gây ra lỗi conflict khi làm việc nhóm.
- Hoạt động:
   - `GameModeContainer.Instance.InitGame()`.
   - `InitController()` → Instantiate module từ `Resources/`.
- GameManager.cs
<img width="1042" height="700" alt="image" src="https://github.com/user-attachments/assets/87d588a0-acda-4b59-a50c-177b712897b8" />

## Design Pattern
## 1) Mô Hình MVC:
- Tất cả đều được thiết kế theo mô hình MVC (Bag, Sell, Shop, Worker, Player, Equiment,...). 
- Đáp ứng nhu cầu tách logic chạy và phần view.
## 2) Singleton cho Manager, Controller.
<img width="650" height="390" alt="image" src="https://github.com/user-attachments/assets/917a42c2-5914-4fff-bdbb-131adc61c2d3" /> </br>
## 3) State cho WorkerModel.
-Tách hành vi theo trạng thái (Idle/Harvest/Produce) khỏi controller, giúp:
  + Luồng công việc rõ ràng, dễ mở rộng thêm state (VD: Deliver, MoveToPlot…).
  + Dễ kiểm thử từng state độc lập.
  + Tránh “god class” ở controller.
- IState.cs
> <img width="453" height="260" alt="image" src="https://github.com/user-attachments/assets/dff4a130-23a4-4d52-8c81-22f4368b26fc" /> </br>

- StateMachine.cs </br>
 > <img width="557" height="367" alt="image" src="https://github.com/user-attachments/assets/2b3cef31-96d9-4e76-97bd-fae7a85dd517" /> </br>

- Idle.cs
>  <img width="590" height="292" alt="image" src="https://github.com/user-attachments/assets/852ff8b8-b422-4a1c-92a8-229c36872505" /> </br>

- Harvest.cs
>  <img width="927" height="456" alt="image" src="https://github.com/user-attachments/assets/a34a8375-bb88-4e4f-ac67-c2a6ab07123f" /> </br>

- Procude.cs
>  <img width="960" height="426" alt="image" src="https://github.com/user-attachments/assets/93d286ce-186a-4368-a382-8a7769a76355" /> </br>

- Cách dùng:
>  <img width="418" height="55" alt="image" src="https://github.com/user-attachments/assets/4275a26a-65df-422a-ba4d-820a05dd4b3a" /> </br>

## UI MANAGER
>> - API generic, type-safe mở UI
>> <img width="365" height="258" alt="image" src="https://github.com/user-attachments/assets/444ecc9f-95d7-4c55-b1a1-a50b1345e978" />

API generic OpenUI<T>(): khởi tạo nếu chưa có, setup và mở UI theo đúng vòng đời.

>> - Kho single-instance theo Type
>> <img width="789" height="102" alt="image" src="https://github.com/user-attachments/assets/2263daeb-1531-49ea-b86a-6b8e678dc319" />

Mỗi loại UI chỉ có một instance đang dùng, tra cứu O(1) theo Type.

>> - Lazy instantiate + gán parent chuẩn
>> <img width="612" height="268" alt="image" src="https://github.com/user-attachments/assets/9daa732c-884d-4b4f-85f1-c6ee37f391b6" />

Chỉ tạo khi cần, tự động đặt đúng CanvasParentTF.

>> - Cache prefab + quét thư mục chuẩn
>> <img width="542" height="492" alt="image" src="https://github.com/user-attachments/assets/51256426-2497-43d1-ad81-ff93edad936f" />

Cache prefab theo Type, chuẩn hóa tài nguyên dưới Resources/UI/.

>> - Back stack theo LIFO + lấy top
>> <img width="426" height="269" alt="image" src="https://github.com/user-attachments/assets/b14f55c1-9c75-459a-a2ef-712cf754a673" />

Luôn thao tác với top.

>> - API đăng ký/huỷ back & quản lý stack
>> <img width="535" height="425" alt="image" src="https://github.com/user-attachments/assets/62feeb71-7f7c-4f68-9fb1-2b366bbbba69" />

Tự khai báo hành vi Back và tự tham gia/ra khỏi stack.

## UI CANVAS
>> - Tự đăng ký Back & vào stack khi setup
>> <img width="448" height="117" alt="image" src="https://github.com/user-attachments/assets/fed3f42f-6eee-4b20-9352-58a089bf99b1" />

Mặc định mỗi UI có Back riêng (BackKey) và vào back stack.

>> - API mở/đóng mặc định, đơn giản mà đủ </br>
>> <img width="512" height="568" alt="image" src="https://github.com/user-attachments/assets/be8211dd-2e5c-4216-98cd-f7658c640651" /> </br>

## CsvConfigLoader.cs
1. Nạp file CSV thành TextAsset từ thư mục Resources.
2. Đọc Header.
3. Mỗi dòng tiếp theo: tách theo dấu phẩy, map theo header.
4. Tạo đối tượng cấu hình và ép kiểu các cột.
<img width="588" height="793" alt="image" src="https://github.com/user-attachments/assets/2585343e-59ec-44ea-a6fb-47804b8dcf07" />


## Mô phỏng hoạt động sản xuất/thu hoạch của worker
# Cách Worker lựa chọn công việc

Logic điều phối được thực hiện trong **`WorkerController`**, với cơ chế duyệt và lấy tất cả **worker đang Idle** để giao việc.  
Thứ tự ưu tiên thực tế như sau:

---

## Ưu tiên 1: Thu hoạch khẩn cấp

1. **Tìm ô (plot) tới hạn sớm nhất hiện tại.**
2. Nếu **không tìm được ô tới hạn sớm nhất**, tức là:
   - Tất cả ô đều **không có deadline** (chưa chín, không thể thu hoạch kịp, ...).  
   👉 **Chuyển sang TÁC VỤ SẢN XUẤT** (ưu tiên sản xuất nhanh và nhiều nhất có thể).
3. Nếu ô này **đã sẵn sàng thu hoạch**:
   - Đã **chín**.
   - **Chưa có worker** nào thu hoạch trước đó.
   - **Chưa quá hạn**.
   - **Deadline vừa đủ lớn hơn (sấp sỉ - khi time task + 1 offset nhỏ)** thời gian thực hiện task của worker (tránh rủi ro bị MISS).  
   👉 **THỰC HIỆN THU HOẠCH NGAY.**

---

## Ưu tiên 2: Thử sản xuất

Nếu **không có ca thu hoạch khẩn cấp**, worker sẽ thử thực hiện tác vụ sản xuất:

1. **Kiểm tra rủi ro deadline:**
   - Nếu cho worker sản xuất **ngay bây giờ**, giả sử khi các worker còn lại chỉ tập trung thu hoạch, kể cả worker hiện tại khi xong task sản xuất, thì có ô (plot) nào **bị MISS deadline** không?
   - Chạy mô phỏng điều trên bằng kỹ thuật 2 con trỏ.
2. **Nếu không có ô bị MISS deadline:**
   - Tìm **hạt giống trong bag** và **ô trống** để sản xuất.
   - Nếu **không tìm được**, fallback sang **tác vụ thu hoạch**.
   - Nếu **tìm được**, **thực hiện tác vụ sản xuất.**
3. **Nếu có ô bị MISS deadline:**
   👉 **Ưu tiên thu hoạch** thay vì sản xuất.

---
# Hàm Quan Trọng
## OnlGame_CheckWorking
1. Hàm này duyệt tất cả Worker đang Idle và gán việc (Harvest/Produce) theo ưu tiên an toàn & hiệu quả, có xét deadline, khả năng sản xuất, và mô phỏng rủi ro sản phẩm hư.
2. GetMinDeadline: Duyệt lấy Plot có Deadline nhỏ nhất:
   <img width="961" height="403" alt="image" src="https://github.com/user-attachments/assets/fce6aef8-b258-42d8-91ec-74a3e034b4b0" />
3. Nếu không lấy được, tức là tất cả ô đều **không có deadline** (chưa chín, không thể thu hoạch kịp, ...) 👉 **Chuyển sang TÁC VỤ SẢN XUẤT** (ưu tiên sản xuất nhanh và nhiều nhất có thể). </br>
   <img width="624" height="324" alt="image" src="https://github.com/user-attachments/assets/b5d73734-f1c2-4307-8fb9-51148c8d5b8c" />

5. Nếu tìm được ô (Đã **chín**, **Chưa có worker** nào thu hoạch trước đó, **Chưa quá hạn**, **Deadline vừa đủ lớn hơn (sấp sỉ: khi time task + 1 offset nhỏ)** thời gian thực hiện task của worker (NGUY HIỂM)  
   👉 **THỰC HIỆN THU HOẠCH NGAY.**
   <img width="894" height="451" alt="image" src="https://github.com/user-attachments/assets/6ab0fd43-df78-4beb-afc6-82815411025b" />

6. Nếu Không thì 👉 **THỬ SẢN XUẤT**.
   - Cho Worker làm thử task sản xuất, giả sử tất cả worker còn lại và chính worker hiện tại khi xong task đều thu hoạch, thì có task nào bị miss hay không (2 con trỏ).
   - <img width="981" height="812" alt="image" src="https://github.com/user-attachments/assets/52996883-6c8a-4cc9-ba08-7b8d9bb38721" />
   - Nếu có ô bị MISS deadline: 👉 **Chuyển sang TÁC VỤ THU HOẠCH**
   - <img width="879" height="307" alt="image" src="https://github.com/user-attachments/assets/75faa776-bdc4-4c45-8f5e-b3a535b92b2e" />
   - Nếu không có ô bị MISS deadline:
     - Tìm **hạt giống trong bag** và **ô trống** để sản xuất.
     - Nếu **không tìm được**, fallback sang **TÁC VỤ THU HOẠCH**.
     - Nếu **tìm được**, **TÁC VỤ SẢN XUẤT.**
     - <img width="967" height="592" alt="image" src="https://github.com/user-attachments/assets/d96758e5-4182-46b8-8a31-cd01575ac5fc" />
## OffGame_CheckWorking
1. Lúc quay lại sau khi tắt game, OffGame_CheckWorking() sẽ mô phỏng những gì có thể đã xảy ra (gieo –> chín –> thu hoạch) dựa trên thời điểm bắt đầu công việc của từng worker, deadline của từng ô (plot) và thời lượng tác vụ (TimeTask), sau đó đổ kết quả về GameData/PlotModelList (Kỹ thuật con trỏ).
2. Dùng mảng đánh dấu để lưu các giá trị quan trọng. </br>
   <img width="823" height="109" alt="image" src="https://github.com/user-attachments/assets/ea6ceeef-3291-48cc-9b01-bcb2d68e983c" /> </br>
3. Lấy thời gian gần nhất Worker có thể tác động lên Plot, So sánh các mốc thời gian (deadline, free-time, worker-finish) → quyết định ưu tiên an toàn. </br>
<img width="522" height="202" alt="image" src="https://github.com/user-attachments/assets/bc779841-8a29-4a73-b043-d5c1fb26f0de" /> </br>
4. Chạy mô phỏng, nếu chọn sản xuất có sản phẩm nào hỏng không, nếu có trả về index Plot chứa sản phẩm đó.
   <img width="655" height="122" alt="image" src="https://github.com/user-attachments/assets/5776be07-0fec-4e73-8b46-2b219543a662" /> </br>
   <img width="1018" height="702" alt="image" src="https://github.com/user-attachments/assets/4e9ae752-e3ca-46b9-a5f3-5114b998fbca" /> </br>
5. Nếu có sản phẩm hỏng khi thực hiện tác vụ sản xuất ở hiện tại, 👉 **Chuyển sang TÁC VỤ THU HOẠCH**
   <img width="797" height="475" alt="image" src="https://github.com/user-attachments/assets/9615aee0-27a8-4e9e-94c2-f56320b1c6fe" />
6. Nếu không thì ưu tiên chọn trống để tối đa sản phẩm:
   - Tìm **hạt giống trong bag** và **ô trống** để sản xuất.
     - Nếu **không tìm được**, fallback sang **TÁC VỤ THU HOẠCH**. </br>
     - <img width="804" height="492" alt="image" src="https://github.com/user-attachments/assets/bd7e4649-ed71-4ab5-8000-75b328de4494" /> </br>
     - Nếu **tìm được**, **TÁC VỤ SẢN XUẤT.** </br>
     - <img width="1033" height="641" alt="image" src="https://github.com/user-attachments/assets/dd9e642e-dde0-464f-ac30-2959cdf2ba46" /> </br>
## GetPlotListFromOldData (Trong PlotController.cs)
- Lấy đúng Data từ trạng thái off game trước đó (Plot rảnh, đang được trồng, đang được thu hoạch, đang phát triển):
- <img width="992" height="643" alt="image" src="https://github.com/user-attachments/assets/2a214af7-ec8a-4131-be54-46ba1f1d39bd" /> </br>
- <img width="1078" height="411" alt="image" src="https://github.com/user-attachments/assets/590217b2-e277-467e-ace5-3d8380d9b2d1" /> </br>
## FillToPlot (Trong WorkerController.cs)
- Đổ lại đúng Data sau khi chạy mô phỏng lên game:
    + Plot: rảnh rỗi, đang được sản xuất, đang được thu hoạch, đang phát triển.
    + Worker: rảnh rỗi, đang sản xuất, đang thu hoạch.
  <img width="636" height="397" alt="image" src="https://github.com/user-attachments/assets/36f8794e-d4fc-4e0a-8dec-ef0bbdbd92f3" /> </br>
  <img width="763" height="422" alt="image" src="https://github.com/user-attachments/assets/d02cdcf9-d6d3-40a9-89af-f424e4626a34" /> </br>
  <img width="863" height="398" alt="image" src="https://github.com/user-attachments/assets/ad754145-04ed-43e2-9a4e-26810f478b29" /> </br>
  <img width="806" height="362" alt="image" src="https://github.com/user-attachments/assets/dbc2d013-185e-4b11-89c9-004bb89f0f81" /> </br>
  <img width="841" height="650" alt="image" src="https://github.com/user-attachments/assets/d84a5d6d-5143-4e3f-9242-9c7941a0e8b4" /> </br>
  <img width="563" height="332" alt="image" src="https://github.com/user-attachments/assets/7c704fde-7e8f-4d8a-a0ba-fb7074484d57" /> </br>
## Link Build
[Build](https://drive.google.com/drive/folders/1q-hbo9AgN4NwjPlr4B_b9DzpRyxr4SS_)




