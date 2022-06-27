/*
 * Privacy page
 */

import '../../shared/shared';
import '../../realtime/signalr';

import React, { useEffect, useState } from "react";
import ReactDOM from "react-dom";
import { Row, Col } from "react-bootstrap";

import '../../shared/sharedlast';

function Page() {

  useEffect(() => {

  }, []); // Fire once

  return (
    <div>
      <Row>
        <Col md={2}></Col>
        <Col className="white-bg">
          <div class="text-center article-title">
            <h1>
              Privacy Policy
            </h1>
          </div>
          <p><h2>What we collect</h2></p>
          <ul>
            <li><b>Information you provide:</b> This includes your username, the cryptographic representation of your password, your email address, user images, user settings, and website content you provide.</li>
            <li><b>Information on your actions:</b> This includes your votes, user ignores, user following, and group following.</li>
            <li><b>Transaction information:</b> This includes a record of your Lightning Network transactions (invoices for deposit and withdraw).</li>
            <li><b>Information automatically collected:</b> This includes timestamps of when you log in, which posts have been viewed (anonymously recorded), and which pages you may navigate to - for the purpose of website user flow and fault monitoring (anonymously).</li>
            <li><b>Information from cookies or other meta sources:</b> This includes information on your preferred language and browser type.  This is collected to present the webpage formatted to your preferences.</li>
            <li>
              <b>Technical application telemetry</b>: For the purposes of monitoring website performance, error detection, and to ensure legal compliance, the following telemetry is collected:
              <ul>
                <li>
                  Uncaught exceptions, including information on: Stack trace,
                  Exception details and message accompanying the error,
                  Line & column number of error,
                  URL where error was raised.
                </li>
                <li>
                  Network Dependency Requests include information on:
                  Url of dependency source,
                  Command & Method used to request the dependency,
                  Duration of the request,
                  Result code and success status of the request,
                  (anonymized) ID (if any) of user making the request,
                  Correlation context (if any) where request is made.
                </li>
                <li>User information (e.g. Location, network, IP)</li>
                <li>Device information (e.g. Browser, OS, version, language, model)</li>
                <li>Session information</li>
              </ul>
            </li>
          </ul>
          <p><h2>How we use the information we collect about you</h2></p>
          <ul>
            <li>To send you email alerts and updates based on your preferences.</li>
            <li>To customize the information presented to you based on your preferences.</li>
            <li>To grant you access to site features (such as moderation and administration features)</li>
            <li>To collect payment and send payments to your wallet.</li>
            <li>To allow other users to send you messages.</li>
            <li>To communicate with you to send you updates and information, based on your preferences.</li>
            <li>To monitor the performance of the website based on your usage and any errors you may experience.</li>
          </ul>
          <p><h2>Sharing our information about you</h2></p>
          <ul>
            <li><b>No sharing with third parties.</b> Zapread.com will not provide any user information, including activities, balances, identities, or emails, to third parties.  With one following exception.</li>
            <li><b>To comply with the law.</b>  Zapread.com may share information in response to a formal request for information if required by applicable law, regulation, legal process or governmental order, including, but not limited to, meeting national security or law enforcement requirements. To the extent the law allows it, we will attempt to provide you with prior notice before disclosing your information.</li>
          </ul>
          <p><h2>Receiving information about you</h2></p>
            <li><b>Information from third parties.</b> Zapread.com may receive some information about you from third parties only given your permission to do so.  This information will be used only for the purpose you permit us to use it for.</li>
            <li><b>Authentication.</b> Zapread.com may receive information such as your user name, alias, or email address from third parties for the purposes of authentication.  This includes logging in with Google, Reddit, Twitter, Facebook, or GitHub.</li>
            <li><b>Payment processing.</b> If you choose to use a credit card subscription, Zapread.com may receive information from our third party payments processor for the purposes of linking purchases to your account.  Zapread.com will not save information made available from third parties beyond what is required to associate your Zapread.com user account to purchases.</li>
          <p><h2>Other</h2></p>
          <ul>
            <li><b>Cookies:</b> Zapread.com uses cookies as necessary to deliver the features of the website.</li>
            <li><b>Advertising:</b> Zapread.com does not host any third party advertising on the website.</li>
            <li><b>Data Retention:</b> Zapread.com will retain data collected by you for as long as necessary given the intended purpose.  If you delete your content, the data may be retained for an unspecified time before full deletion from our records.</li>
            <li><b>Children:</b> You must be over the age required by the laws of your country to create an account or otherwise use the website, or we need to have obtained verifiable consent from your parent or legal guardian.</li>
          </ul>
        </Col>
        <Col md={2}></Col>
      </Row>
    </div>
  );
}

ReactDOM.render(<Page />, document.getElementById("root"));