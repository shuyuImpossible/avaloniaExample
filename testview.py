import subprocess
import psutil
import time
import pygetwindow
from pynput import keyboard
import pyautogui
import tkinter as tk
from tkinter import simpledialog
from tkinter import messagebox
import json
import os
import sys
import csv
import uuid

program_path = "C:\\dws\\systeminformer-3.2.25011-release-bin\\amd64\\SystemInformer.exe"
program_window_title = "System Informer"
pos_dict_file = "pos_dict.json"
exec_dict_file = "exec_dict.json"

ctrl_pressed = False
pos_dict = {}
exec_list = []


if os.path.exists(pos_dict_file):
    try:
        with open(pos_dict_file, 'r') as f:
            pos_dict = json.load(f)
    except json.JSONDecodeError:
        print("Error reading pos_dict.json, starting with an empty dictionary.")
        sys.exit(1)
    except Exception as e:
        print(f"Unexpected error reading pos_dict.json: {e}")
        sys.exit(1)

if os.path.exists(exec_dict_file):
    try:
        with open(exec_dict_file, 'r') as f:
            exec_list = json.load(f)
    except json.JSONDecodeError:
        print("Error reading exec_dict.json, starting with an empty dictionary.")
        sys.exit(1)
    except Exception as e:
        print(f"Unexpected error reading exec_dict.json: {e}")
        sys.exit(1)

def on_press(key):
    global ctrl_pressed
    if ctrl_pressed:
        return
    try:
        if key == keyboard.Key.ctrl_l:
            ctrl_pressed = True
            x, y = pyautogui.position()
            print(f"Mouse position: ({x}, {y})")

            # 弹出对话框收集用户输入
            root = tk.Tk()
            root.withdraw()  # 隐藏主窗口
            user_input = simpledialog.askstring(title="Input", prompt="Enter a string:")
            if user_input is not None:
                if user_input in pos_dict:
                    print(f"User input already exists: {user_input}")
                    messagebox.showerror("Error", "User input already exists." + user_input)
                else:
                    print(f"User input: {user_input}")
                    windows = pygetwindow.getWindowsWithTitle(program_window_title)
                    if windows:
                        window = windows[0]
                        print(f"xxx Window Position: (Left: {window.left}, Top: {window.top}, Width: {window.width}, Height: {window.height})")
                        if window.isMinimized:
                            print("Window is minimized.")
                            messagebox.showerror("Error", "Window is minimized." + user_input)
                        else:
                            xoffset = x - window.left
                            yoffset = y - window.top
                            print(f"xxx Offset: ({xoffset}, {yoffset})")
                            pos_dict[user_input] = (xoffset, yoffset)
            root.destroy()
    except AttributeError:
        pass

def on_release(key):
    global ctrl_pressed
    if key == keyboard.Key.ctrl_l:
        ctrl_pressed = False

# 启动键盘监听器
listener = keyboard.Listener(on_press=on_press, on_release=on_release)

def monitor_loop(ps_process):
    threadcpudict = {}
    lastnetinterfacedict = {}
    unique_id = str(uuid.uuid4().hex)
    csv_file = open('process_data'+time.strftime("_%Y-%m-%d-%H-%M-%S_")+unique_id+'.csv', 'w', newline='')
    csv_writer = csv.writer(csv_file)
    csv_writer.writerow(['Timestamp', 'CPU Usage', 'Memory Usage (MB)', 'Disk Read (MB)', 'Disk Write (MB)', 'Network Sent (MB)', 'Network Received (MB)'])

    lastDiskReadBytes = 0
    lastDiskWriteBytes = 0
    try:
        while True:
            timestamp = time.strftime("%Y-%m-%d %H:%M:%S")

            # 获取 CPU 使用率
            cpu_usage = ps_process.cpu_percent(interval=1)
            
            # 获取内存使用情况
            memory_info = ps_process.memory_info()
            memory_usage = memory_info.rss  # 常驻内存使用量
            
            # 获取磁盘 I/O 使用情况
            io_counters = ps_process.io_counters()
            read_bytes_total = io_counters.read_bytes
            read_bytes = read_bytes_total - lastDiskReadBytes
            lastDiskReadBytes = read_bytes_total
            write_bytes_total = io_counters.write_bytes
            write_bytes = write_bytes_total - lastDiskWriteBytes
            lastDiskWriteBytes = write_bytes_total

            # 获取网络 I/O 使用情况
            connections = ps_process.net_connections(kind="tcp")
            net_io = psutil.net_io_counters(pernic=True)
            netbytes_sent_total = 0
            netbytes_recv_total = 0

            currentnetinterfacedict = {}
            for conn in connections:
                laddr = conn.laddr
                raddr = conn.raddr
                if laddr and raddr:
                    interface = laddr.ip
                    if interface in net_io:
                        sent_bytes = net_io[interface].bytes_sent
                        recv_bytes = net_io[interface].bytes_recv
                        print(f"Connection from {laddr} to {raddr}: Sent {sent_bytes} bytes, Received {recv_bytes} bytes")
                        sent_bytes_diff = 0
                        recv_bytes_diff = 0
                        if interface in lastnetinterfacedict:
                            last_sent_bytes, last_recv_bytes = lastnetinterfacedict[interface]
                            sent_bytes_diff = sent_bytes - last_sent_bytes
                            recv_bytes_diff = recv_bytes - last_recv_bytes
                        else:
                            print(f"New connection from {laddr} to {raddr}.")
                            sent_bytes_diff = sent_bytes
                            recv_bytes_diff = recv_bytes
                        currentnetinterfacedict[interface] = (sent_bytes, recv_bytes)
                        netbytes_sent_total += sent_bytes_diff
                        netbytes_recv_total += recv_bytes_diff
            for interface in lastnetinterfacedict:
                if interface not in currentnetinterfacedict:
                    print(f"Closed connection from {interface}")
            lastnetinterfacedict = currentnetinterfacedict

            # threads = ps_process.threads()
            # print(f"Number of threads: {len(threads)}")
            # for thread in threads:
            #     if thread.id in threadcpudict:
            #         last_user_time, last_system_time = threadcpudict[thread.id]
            #         user_time_diff = thread.user_time - last_user_time
            #         system_time_diff = thread.system_time - last_system_time
            #         if user_time_diff > 0 or system_time_diff > 0:
            #             print(f"Thread ID: {thread.id}, User Time Diff: {user_time_diff}, System Time Diff: {system_time_diff}")
            #     else:
            #         print(f"New Thread ID: {thread.id}, User Time: {thread.user_time}, System Time: {thread.system_time}")
            #     threadcpudict[thread.id] = (thread.user_time, thread.system_time)
            
            # 打印资源使用情况
            # print(f"CPU Usage: {cpu_usage}%")
            # print(f"Memory Usage: {memory_usage / (1024 * 1024):.2f} MB")
            # print(f"Disk I/O - Read: {read_bytes / (1024 * 1024):.2f} MB, Write: {write_bytes / (1024 * 1024):.2f} MB")
            
            csv_writer.writerow([timestamp, cpu_usage, float(memory_usage)/(1024.0*1024.0), read_bytes, write_bytes, netbytes_sent_total, netbytes_recv_total])

            
    except psutil.NoSuchProcess:
        print("Process terminated.")
    finally:
        csv_file.close()
        print("CSV file saved as process_data.csv")

def exec_loop():
    global exec_list
    # first check values are all valid
    isValid = True
    for kv in exec_list:
        for key, value in kv.items():
            print(f"checking command: {key}, {value}")
            if key == "mouse_move":
                if value not in pos_dict:
                    isValid = False
                    print(f"Invalid command: {key}, {value}")
            if key == "mouse_click":
                if len(value) != 2 or value[0] not in pos_dict or int(value[1]) <= 0:
                    isValid = False
                    print(f"Invalid command: {key}, {value}")
            if key == "sleep":
                if float(value) <= 0:
                    isValid = False
                    print(f"Invalid command: {key}, {value}")
            if key == "key_presse":
                if value not in pyautogui.KEYBOARD_KEYS:
                    isValid = False
                    print(f"Invalid command: {key}, {value}")
    if not isValid:
        sys.exit(1)

    for kv in exec_list:
        print("exec_list: ", kv)
        for key, value in kv.items():
            print(f"Executing command: {key}, {value}")
            windows = pygetwindow.getWindowsWithTitle(program_window_title)
            wleft = 0
            wtop = 0
            time.sleep(5)
            if windows:
                window = windows[0]
                if not window:
                    print("Window not found.")
                    sys.exit(1)
                if window.isMinimized:
                    print("Window is minimized.")
                    sys.exit(1)
                wleft = window.left
                wtop = window.top
                print(f"Window position: (Left: {wleft}, Top: {wtop})")

            print(f"Executing command: {key}, {value}")
            if key == "mouse_move":
                mousex = wleft+pos_dict[value][0]
                mousey = wtop+pos_dict[value][1]
                print(f"mouse_move: (x: {mousex}, y: {mousey})")
                pyautogui.moveTo(mousex, mousey, duration=0.25)
            if key == "mouse_click":
                mousex = wleft+pos_dict[value[0]][0]
                mousey = wtop+pos_dict[value[0]][1]
                print(f"mouse_click: (x: {mousex}, y: {mousey}, clicks: {value[1]})")
                pyautogui.click(mousex, mousey, clicks=int(value[1]), interval=0.25)
            if key == "sleep":
                time.sleep(float(value))
            if key == "key_presse":
                pyautogui.press(value)

try:
    if len(sys.argv) > 1:
        root = tk.Tk()
        root.withdraw()
        messagebox.showinfo("mouse pos", "sample mouse pos.")
        root.destroy()
        listener.start()
        process = subprocess.Popen([program_path])
        ps_process = psutil.Process(process.pid)
        while True:
            time.sleep(100)
    else:
        while True:
            process = subprocess.Popen([program_path])
            ps_process = psutil.Process(process.pid)
            exec_loop()
            print("enter monitorloop!!!!")
            monitor_loop(ps_process)
finally:
    # 保存 pos_dict 到文件
    with open(pos_dict_file, 'w') as f:
        json.dump(pos_dict, f)
    print("pos_dict saved to pos_dict.json")