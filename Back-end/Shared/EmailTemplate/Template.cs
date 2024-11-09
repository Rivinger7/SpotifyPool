using System.Text.Encodings.Web;

namespace Utility.EmailTemplate
{
    public class Template
    {
        public static string BodyEmailConfirmTemplate(string displayName, string message)
        {
            return $@"
                <body
  style='height: 100vh; width: 100%; background: #000; font-size: 16px; font-family: sans-serif'
>
  <div
    style='width: 100%; height: 100%; display: flex; align-items: center; justify-content: center'
  >
    <div
      style='
        padding: 24px;
        min-width: 320px;
        max-width: 512px;
        background: #fff;
        border-radius: 6px;
      '
    >
      <div>
        <img
          style='
            display: flex;
            justify-content: center;
            margin: 0 auto;
            width: 40px;
            height: 40px;
            object-fit: cover;
            object-position: center;
            border-radius: 50%;
          '
          src='https://storage.googleapis.com/pr-newsroom-wp/1/2023/05/Spotify_Primary_Logo_RGB_Black.png'
          alt='spotify icon black'
        />
      </div>
      <h1
        style='
          font-size: 28px;
          text-align: center;
          line-height: 36px;
          font-weight: bold;
          margin-top: 16px;
        '
      >
        Confirm your email address
      </h1>
      <p class='confirm-username' style='margin-top: 8px; text-align: center'>Dear {displayName},</p>
      <p style='margin-top: 8px; text-align: center'>
        You have confirmed the email address for your account. Please verify the email to
        confirm.
      </p>
      <a style='display: block; margin-top: 16px' href='{HtmlEncoder.Default.Encode(message)}'>
        <button
          style='
            display: inline-flex;
            align-items: center;
            justify-content: center;
            border: none;
            outline: none;
            background: #000;
            text-align: center;
            width: 100%;
            height: 40px;
            font-size: 16px;
            padding: 8px 16px;
            color: #fff;
            border-radius: 6px;
            font-weight: 500;
            white-space: nowrap;
            cursor: pointer;
            transition: all 0.15s ease-in-out;
          '
          onmouseover='this.style.backgroundColor='rgba(0, 0, 0, 0.8)''
          onmouseout='this.style.backgroundColor='rgb(0, 0, 0)''
        >
          Confirm your email
        </button>
      </a>
      <p style='margin-top: 8px; font-size: 12px; color: #919191'>
        If you did not request this, please change your account password immediately to prevent
        unauthorized access. If you need assistance, please contact us through the
        <a
          href='mailto:Rivinger7@gmail.com'
          style='color: #1ed760; text-decoration: none; transition: all 0.15s ease-in-out'
          target='_blank'
          onmouseover='this.style.color='#121212'; this.style.textDecoration='underline''
          onmouseout='this.style.color='#1ed760'; this.style.textDecoration='none''
        >
          SpotifyPool Support Service.
        </a>
      </p>
    </div>
  </div>
</body>
                ";
        }

        public static string BodyEmailForgotPasswordTemplate(string otpToEmail)
        {
            return $"Your OTP is: {otpToEmail}";
        }

        public static string BodyEmailResetPasswordTemplate(string password)
        {
            return $"Your new password is: {password}";
        }
    }
}
