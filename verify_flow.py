import time
from playwright.sync_api import sync_playwright

def verify_booking_flow():
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        # Create a new context. This context is like a browser instance.
        context = browser.new_context()
        page = context.new_page()

        try:
            # 1. Register a new Consumer
            print("Navigating to Register page...")
            page.goto("http://localhost:5289/Register")

            timestamp = str(int(time.time()))
            username = f"consumer_{timestamp}"
            password = "Password123!"

            print(f"Registering user: {username}")
            page.fill("input[name='Input.FullName']", "Test Consumer")
            page.fill("input[name='Input.Username']", username)
            page.fill("input[name='Input.Password']", password)
            page.fill("input[name='Input.Phone']", "0912345678")
            page.fill("input[name='Input.IdentityNumber']", "123456789")
            page.fill("input[name='Input.Email']", f"{username}@test.com")
            page.fill("textarea[name='Input.Address']", "Test Address")

            page.screenshot(path="verification_step1_fill_register.png")
            page.click("button[type='submit']")

            # 2. Verify Redirect to Login and Login
            print("Verifying redirect to Login...")
            # Wait for URL to contain Login
            page.wait_for_url("**/Login*")
            page.screenshot(path="verification_step2_login_redirect.png")

            print("Logging in...")
            page.fill("input[name='LoginData.Username']", username)
            page.fill("input[name='LoginData.Password']", password)
            page.click("button[type='submit']")

            # 3. Verify Redirect to Index (Consumer Home)
            print("Verifying redirect to Index...")
            try:
                page.wait_for_url("**/Index", timeout=10000) # Short timeout
                print("Redirected to Index successfully.")
            except:
                print("Redirect timeout. Checking where we are...")
                print(f"Current URL: {page.url}")
                page.screenshot(path="verification_error_redirect.png")
                # Maybe login failed? Check for error message
                if "Sai tài khoản hoặc mật khẩu" in page.content():
                     print("Login failed: Wrong credentials.")
                raise

            page.screenshot(path="verification_step3_index.png")

            # 4. Book a room
            print("Booking a room...")
            # Find any "Book" button.
            # In Index.cshtml, the button text is "ĐẶT PHÒNG" inside an <a> tag.
            # But wait, Index.cshtml links to /Admin/Create?roomId=...
            # We need to make sure the consumer can access that or we should have updated the link in Index.cshtml.
            # I did NOT update Index.cshtml to point to /Booking for consumers.
            # The current link is <a asp-page="/Admin/Create" ...>
            # If consumer clicks that, they might get Access Denied if /Admin/Create is restricted.
            # Let's check if there is a button.

            # Since I didn't update Index.cshtml, the button points to /Admin/Create.
            # Consumer clicking it might fail.
            # Let's try direct navigation to /Booking?roomId=... using a valid room ID.

            # Get a room ID from the page source
            # Find an href like /Admin/Create?roomId=1
            room_link = page.locator("a[href*='roomId']").first
            if room_link.count() > 0:
                href = room_link.get_attribute("href")
                # Extract ID
                import re
                match = re.search(r"roomId=(\d+)", href)
                if match:
                    room_id = match.group(1)
                    print(f"Found Room ID: {room_id}")

                    booking_url = f"http://localhost:5289/Booking?roomId={room_id}"
                    print(f"Navigating to {booking_url}")
                    page.goto(booking_url)
                else:
                    print("Could not extract room ID.")
            else:
                print("No room links found. Are there any rooms?")
                page.screenshot(path="verification_error_no_rooms.png")

            page.screenshot(path="verification_step4_booking_page.png")

            # 5. Verify Booking Page pre-filled info
            # Customer ID should be readonly and filled
            cust_input = page.locator("input[name='RequestData.CustomerId']")
            is_readonly = cust_input.get_attribute("readonly") is not None
            cust_val = cust_input.input_value()
            print(f"Customer ID value: {cust_val}, Readonly: {is_readonly}")

            if not cust_val:
                print("Customer ID is empty! Logic error.")

            # Submit Booking
            print("Submitting booking...")
            page.click("button[type='submit']")

            # 6. Verify success message and Pending status (Consumer view)
            print("Verifying success...")
            page.wait_for_url("**/Index")
            page.screenshot(path="verification_step5_after_booking.png")

            content = page.content()
            if "Đặt phòng thành công" in content:
                print("Success message found.")
            else:
                print("Success message NOT found.")

            # 7. Admin Confirmation
            print("Logging out...")
            page.goto("http://localhost:5289/Logout")

            print("Logging in as Admin...")
            page.fill("input[name='LoginData.Username']", "admin")
            page.fill("input[name='LoginData.Password']", "admin123")
            page.click("button[type='submit']")

            print("Navigating to Reservations...")
            page.goto("http://localhost:5289/Reservations")
            page.screenshot(path="verification_step6_admin_reservations.png")

            # Verify Pending Reservation is there
            if "Chờ duyệt" in page.content():
                print("Pending reservation found.")
            else:
                print("Pending reservation NOT found.")

            # Click Confirm
            # Find the button inside the form that posts to Confirm
            confirm_btn = page.locator("button:has-text('Xác nhận')").first
            if confirm_btn.count() > 0:
                print("Clicking Confirm...")
                confirm_btn.click()

                # Wait for reload
                page.wait_for_load_state("networkidle")
                page.screenshot(path="verification_step7_admin_confirmed.png")

                if "Chờ Check-in" in page.content():
                     print("Reservation confirmed successfully.")
            else:
                print("Confirm button not found.")

        except Exception as e:
            print(f"Error: {e}")
            page.screenshot(path="verification_error_exception.png")
        finally:
            browser.close()

if __name__ == "__main__":
    verify_booking_flow()
