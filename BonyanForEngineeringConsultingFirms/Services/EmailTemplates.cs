namespace BonyanForEngineeringConsultingFirms.Services
{
	public static class EmailTemplates
	{
		// ════════════════════════════════════════════════
		//  EMAIL SENT TO ADMIN — when user forgets password
		// ════════════════════════════════════════════════

		public static string ForgotPasswordAdminNotification(
			string userFullName,
			string userEmail,
			string resetLink)
		{
			return $@"
<!DOCTYPE html>
<html lang='ar' dir='rtl'>
<head>
    <meta charset='UTF-8' />
    <style>
        body {{
            font-family: Cairo, Arial, sans-serif;
            background: #F4F6F9;
            margin: 0;
            padding: 30px;
        }}
        .email-card {{
            background: #ffffff;
            border-radius: 16px;
            max-width: 560px;
            margin: auto;
            overflow: hidden;
            box-shadow: 0 4px 20px rgba(0,0,0,0.08);
        }}
        .email-header {{
            background: linear-gradient(135deg, #1B3A6B, #112240);
            padding: 30px;
            text-align: center;
        }}
        .email-header h1 {{
            color: #ffffff;
            font-size: 1.6rem;
            margin: 0 0 6px 0;
        }}
        .email-header p {{
            color: rgba(255,255,255,0.7);
            margin: 0;
            font-size: 0.9rem;
        }}
        .email-body {{
            padding: 30px;
        }}
        .email-body p {{
            color: #444;
            line-height: 1.8;
            font-size: 0.95rem;
        }}
        .info-box {{
            background: #F4F6F9;
            border-right: 4px solid #1B3A6B;
            border-radius: 8px;
            padding: 14px 18px;
            margin: 20px 0;
        }}
        .info-box p {{
            margin: 4px 0;
            color: #333;
            font-size: 0.9rem;
        }}
        .info-box strong {{
            color: #1B3A6B;
        }}
        .btn-reset {{
            display: inline-block;
            background: linear-gradient(135deg, #1B3A6B, #2a5298);
            color: #ffffff !important;
            text-decoration: none;
            padding: 14px 28px;
            border-radius: 10px;
            font-size: 0.95rem;
            font-weight: 700;
            margin-top: 10px;
        }}
        .email-footer {{
            background: #F4F6F9;
            padding: 16px 30px;
            text-align: center;
            color: #999;
            font-size: 0.8rem;
            border-top: 1px solid #eee;
        }}
    </style>
</head>
<body>
    <div class='email-card'>

        <div class='email-header'>
            <h1>🔐 بنيان للاستشارات الهندسية</h1>
            <p>طلب استعادة كلمة المرور</p>
        </div>

        <div class='email-body'>
            <p>مرحباً أيها المدير،</p>
            <p>
                قام أحد المستخدمين بطلب استعادة كلمة المرور.
                يُرجى مراجعة البيانات أدناه واتخاذ الإجراء المناسب.
            </p>

            <div class='info-box'>
                <p>👤 <strong>الاسم:</strong> {userFullName}</p>
                <p>📧 <strong>البريد الإلكتروني:</strong> {userEmail}</p>
            </div>

            <p>لإعادة تعيين كلمة المرور، اضغط على الزر أدناه:</p>
            <a href='{resetLink}' class='btn-reset'>
                إعادة تعيين كلمة المرور
            </a>

            <p style='margin-top:24px; color:#888; font-size:0.85rem;'>
                إذا لم تتعرف على هذا الطلب، يُرجى تجاهل هذا البريد.
            </p>
        </div>

        <div class='email-footer'>
            نظام بنيان للاستشارات الهندسية &mdash; جميع الحقوق محفوظة
        </div>

    </div>
</body>
</html>";
		}

		// ════════════════════════════════════════════════
		//  EMAIL SENT TO USER — after admin resets password
		// ════════════════════════════════════════════════

		public static string NewPasswordNotification(
			string userFullName,
			string newPassword)
		{
			return $@"
<!DOCTYPE html>
<html lang='ar' dir='rtl'>
<head>
    <meta charset='UTF-8' />
    <style>
        body {{
            font-family: Cairo, Arial, sans-serif;
            background: #F4F6F9;
            margin: 0;
            padding: 30px;
        }}
        .email-card {{
            background: #ffffff;
            border-radius: 16px;
            max-width: 560px;
            margin: auto;
            overflow: hidden;
            box-shadow: 0 4px 20px rgba(0,0,0,0.08);
        }}
        .email-header {{
            background: linear-gradient(135deg, #1B3A6B, #112240);
            padding: 30px;
            text-align: center;
        }}
        .email-header h1 {{
            color: #ffffff;
            font-size: 1.6rem;
            margin: 0 0 6px 0;
        }}
        .email-header p {{
            color: rgba(255,255,255,0.7);
            margin: 0;
            font-size: 0.9rem;
        }}
        .email-body {{
            padding: 30px;
        }}
        .email-body p {{
            color: #444;
            line-height: 1.8;
            font-size: 0.95rem;
        }}
        .password-box {{
            background: #F4F6F9;
            border: 2px dashed #1B3A6B;
            border-radius: 12px;
            padding: 20px;
            text-align: center;
            margin: 20px 0;
        }}
        .password-box p {{
            margin: 0 0 8px 0;
            color: #888;
            font-size: 0.85rem;
        }}
        .password-box h2 {{
            color: #1B3A6B;
            letter-spacing: 3px;
            font-size: 1.6rem;
            margin: 0;
        }}
        .warning-box {{
            background: #FFF8E1;
            border-right: 4px solid #C8922A;
            border-radius: 8px;
            padding: 12px 16px;
            margin: 20px 0;
        }}
        .warning-box p {{
            color: #856404;
            margin: 0;
            font-size: 0.88rem;
        }}
        .email-footer {{
            background: #F4F6F9;
            padding: 16px 30px;
            text-align: center;
            color: #999;
            font-size: 0.8rem;
            border-top: 1px solid #eee;
        }}
    </style>
</head>
<body>
    <div class='email-card'>

        <div class='email-header'>
            <h1>🔑 بنيان للاستشارات الهندسية</h1>
            <p>تم إعادة تعيين كلمة المرور</p>
        </div>

        <div class='email-body'>
            <p>مرحباً <strong>{userFullName}</strong>،</p>
            <p>
                تم إعادة تعيين كلمة المرور الخاصة بك من قِبل المدير.
                يمكنك الآن تسجيل الدخول باستخدام كلمة المرور المؤقتة التالية:
            </p>

            <div class='password-box'>
                <p>كلمة المرور المؤقتة</p>
                <h2>{newPassword}</h2>
            </div>

            <div class='warning-box'>
                <p>
                    ⚠️ يُرجى تغيير كلمة المرور فور تسجيل الدخول.
                    لن تتمكن من الوصول إلى النظام قبل تغييرها.
                </p>
            </div>

            <p style='color:#888; font-size:0.85rem;'>
                إذا لم تطلب إعادة تعيين كلمة المرور، يُرجى التواصل مع المدير فوراً.
            </p>
        </div>

        <div class='email-footer'>
            نظام بنيان للاستشارات الهندسية &mdash; جميع الحقوق محفوظة
        </div>

    </div>
</body>
</html>";
		}
	}
}