CÁCH ĐỂ DÙNG AI:
1. tạo 1 folder riêng biệt và thêm file main.py và tạo thêm 1 file .env
2. trong .env bỏ dòng lệnh này vào "GEMINI_API_KEY=bỏ api key vào đây"
3. tải những tool cần thiết như:
pip uninstall google-generativeai -y,
pip install google-genai,
pip install uvicorn
5. mở terminal của folder này và gõ lệnh: uvicorn main:app --reload

XONG. Thế là cứ vào web và test thôi
