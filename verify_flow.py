from playwright.sync_api import sync_playwright
import time
import re

def run():
    print("Starting verification script...")
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        context = browser.new_context()
        page = context.new_page()

        try:
            # 1. Register a new user
            print("Navigating to Register page...")
            try:
                page.goto("http://localhost:3000/Register", timeout=30000)
            except Exception as e:
                print(f"Failed to load Register page: {e}")
                return

            timestamp = int(time.time())
            username = f"user_{timestamp}"

            print(f"Registering user: {username}")
            page.fill("input[name='Input.FullName']", "Test User")
            page.fill("input[name='Input.Username']", username)
            page.fill("input[name='Input.Password']", "Password123!")
            page.fill("input[name='Input.Phone']", "0987654321")
            page.fill("input[name='Input.IdentityNumber']", f"ID{timestamp}")
            page.fill("input[name='Input.Email']", f"{username}@example.com")
            page.fill("textarea[name='Input.Address']", "123 Test Street")

            print("Submitting registration...")
            with page.expect_navigation(url="**/Login*"):
                page.click("button[type='submit']")
            print("Registration successful, redirected to Login.")

            # 2. Login
            print("Logging in...")
            page.fill("input[name='LoginData.Username']", username)
            page.fill("input[name='LoginData.Password']", "Password123!")

            with page.expect_navigation():
                page.click("button[type='submit']")

            print(f"Login successful. Current URL: {page.url}")

            # 3. Book a room
            print("Finding available room...")
            book_links = page.locator("a.btn-outline-success", has_text="ĐẶT PHÒNG")
            count = book_links.count()
            if count == 0:
                print("No available rooms found to book.")
                page.screenshot(path="verification_no_rooms.png")
                return

            print(f"Found {count} available rooms. Booking the first one.")
            book_link = book_links.first
            room_id = book_link.get_attribute("href").split("=")[-1]

            with page.expect_navigation():
                book_link.click()

            print(f"On Booking page for room {room_id}")

            # Fill booking dates
            page.evaluate("""
                const today = new Date();
                const tomorrow = new Date(today);
                tomorrow.setDate(tomorrow.getDate() + 1);
                const dayAfter = new Date(tomorrow);
                dayAfter.setDate(dayAfter.getDate() + 1);

                document.querySelector('input[name="RequestData.CheckInDate"]').valueAsDate = tomorrow;
                document.querySelector('input[name="RequestData.CheckOutDate"]').valueAsDate = dayAfter;
            """)

            print("Submitting booking...")
            with page.expect_navigation():
                page.click("button:has-text('Xác nhận đặt phòng')")
            print("Booking submitted.")

            # 4. Admin approval needed
            print("Logging out Consumer...")
            page.goto("http://localhost:3000/Logout")

            print("Logging in Admin...")
            page.goto("http://localhost:3000/Login")
            page.fill("input[name='LoginData.Username']", "admin")
            # Update password
            page.fill("input[name='LoginData.Password']", "admin_password_placeholder")
            with page.expect_navigation():
                page.click("button[type='submit']")

            print("Navigating to Reservations...")
            page.goto("http://localhost:3000/Reservations")
            page.screenshot(path="verification_admin_reservations.png") # Debug

            # Find the pending reservation.
            confirm_btns = page.locator("button:has-text('Xác nhận')")
            if confirm_btns.count() > 0:
                print("Confirming reservation...")
                with page.expect_navigation():
                    confirm_btns.first.click()
                print("Confirmed.")
            else:
                print("No pending reservation to confirm. Checking if already confirmed.")

            # Now Check In
            checkin_btns = page.locator("button:has-text('Check-in')")
            if checkin_btns.count() > 0:
                print("Checking in reservation...")
                with page.expect_navigation():
                    checkin_btns.first.click()
                print("Checked In.")
            else:
                print("No confirmed reservation to check in.")

            print("Logging out Admin...")
            page.goto("http://localhost:3000/Logout")

            # 5. Login Consumer again
            print("Logging in Consumer again...")
            page.goto("http://localhost:3000/Login")
            page.fill("input[name='LoginData.Username']", username)
            page.fill("input[name='LoginData.Password']", "Password123!")
            with page.expect_navigation():
                page.click("button[type='submit']")

            # 6. Verify "Return Room" / "Check Out" button
            print("Verifying Check Out button...")

            page.goto("http://localhost:3000/Index")

            checkout_btn = page.locator("a.btn-danger", has_text="TRẢ PHÒNG").first
            if checkout_btn.is_visible():
                print("Check Out button is visible!")

                with page.expect_navigation():
                    checkout_btn.click()

                print(f"On Page: {page.url}")
                # Check for payment form elements
                if page.locator("input[name='PaymentInput.CardNumber']").is_visible():
                    print("Payment form visible.")
                    page.screenshot(path="verification_payment.png")
                    print("Screenshot taken: verification_payment.png")

                    # Fill Payment Form
                    page.fill("input[name='PaymentInput.CardHolderName']", "Test User")
                    page.fill("input[name='PaymentInput.CardNumber']", "4111 1111 1111 1111")
                    page.fill("input[name='PaymentInput.ExpiryDate']", "12/25")
                    page.fill("input[name='PaymentInput.CVV']", "123")

                    # Submit Payment
                    print("Submitting payment...")
                    page.on("dialog", lambda dialog: dialog.accept())

                    with page.expect_navigation():
                        page.click("button:has-text('THANH TOÁN NGAY')")
                    print("Redirected after payment.")

                    # Verify Room is no longer occupied by user (button gone)
                    if not page.locator("a.btn-danger", has_text="TRẢ PHÒNG").is_visible():
                        print("Check Out button is gone. Success!")
                    else:
                        print("Check Out button still visible. Failed?")
                else:
                    print("Payment form NOT visible.")
                    page.screenshot(path="verification_error_payment.png")

            else:
                print("Check Out button NOT found.")
                page.screenshot(path="verification_error_button.png")

        except Exception as e:
            print(f"Error: {e}")
            page.screenshot(path="verification_error_exception.png")
        finally:
            browser.close()

if __name__ == "__main__":
    run()
