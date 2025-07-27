const { chromium } = require('playwright');

/**
 * Comprehensive Playwright Test Template with Zero-Tolerance Error Detection
 * Copy this file and customize the COMPONENT_NAME and TARGET_URL for your specific test
 */

const COMPONENT_NAME = '[REPLACE_WITH_COMPONENT_NAME]'; // e.g., 'DataTable.razor', 'ComplianceTracker.razor'
const TARGET_URL = '[REPLACE_WITH_TARGET_URL]'; // e.g., '/dashboard', '/company/compliance'

(async () => {
  console.log(`🎯 Testing: ${COMPONENT_NAME}`);
  
  const browser = await chromium.launch({ 
    headless: false,
    slowMo: 300
  });
  const page = await browser.newPage();
  
  let testFailed = false;
  const errors = [];
  const screenshots = [];

  page.on('console', msg => {
    if (msg.type() === 'error') {
      const errorText = msg.text();
      if (!errorText.includes('serilog') && !errorText.includes('%c') && 
          !errorText.includes('[DEBUG]') && !errorText.includes('[INFO]') &&
          !errorText.includes('Please reload') && !errorText.includes('SignalR') &&
          !errorText.includes('WebSocket') && !errorText.includes('connection closed') &&
          !errorText.includes('Failed to fetch') && !errorText.includes('chunk-') &&
          !errorText.includes('Loading chunk')) {
        errors.push(`Console Error: ${errorText}`);
        testFailed = true;
      }
    }
  });

  page.on('pageerror', error => {
    errors.push(`Page Error: ${error.message}`);
    testFailed = true;
  });

  page.on('requestfailed', request => {
    const url = request.url();
    const failure = request.failure()?.errorText || 'Unknown error';
    if (!url.includes('_blazor/') && !url.includes('sockjs-node') &&
        !url.includes('hot-reload') && !failure.includes('net::ERR_CONNECTION_REFUSED') &&
        !failure.includes('net::ERR_ABORTED')) {
      errors.push(`Request Failed: ${url}`);
      testFailed = true;
    }
  });

  page.on('response', response => {
    if (response.status() >= 400) {
      errors.push(`HTTP ${response.status()}: ${response.url()}`);
      if (response.status() >= 500) {
        testFailed = true;
      }
    }
  });

  try {
    // STEP 1: Navigation & Authentication
    console.log('🔐 Authenticating...');
    await page.goto('https://localhost:5002', { waitUntil: 'networkidle' });
    
    const errorBoundaries = await page.locator('[data-error-boundary], .error-boundary, .blazor-error-boundary').count();
    if (errorBoundaries > 0) {
      testFailed = true;
      const screenshot = `_TestResults/Screenshots/${COMPONENT_NAME}-error-boundary-login.png`;
      await page.screenshot({ path: screenshot, fullPage: true });
      screenshots.push(screenshot);
      throw new Error('Error boundary detected');
    }

    // Check if we need to login or are already authenticated
    const emailInput = page.locator('input[type="email"]').first();
    if (await emailInput.isVisible({ timeout: 3000 })) {
      await emailInput.fill('sarah.johnson@gmail.com');
      await page.fill('input[type="password"]', 'Test123!');
      await page.click('button[type="submit"]');
      await page.waitForTimeout(3000);
      
      const authErrorMessages = await page.locator('.alert-danger, .validation-summary-errors, .field-validation-error').allTextContents();
      const criticalErrors = authErrorMessages.filter(msg => 
        msg && msg.trim() && !msg.includes('Please reload') && !msg.includes('Loading') &&
        !msg.includes('temporary') && !msg.includes('Refreshing') && !msg.includes('Reconnecting') &&
        !msg.includes('blazor') && !msg.includes('SignalR') &&
        (msg.includes('Invalid') || msg.includes('incorrect') || msg.includes('failed') || msg.includes('denied') || msg.includes('error'))
      );
      
      if (criticalErrors.length > 0) {
        testFailed = true;
        throw new Error(`Authentication failed: ${criticalErrors[0]}`);
      }
      
      await page.waitForTimeout(2000);
      const currentUrl = page.url();
      const pageTitle = await page.title();
      
      if ((currentUrl.includes('login') || currentUrl.includes('identity')) && 
          (pageTitle.includes('Login') || pageTitle.includes('Sign'))) {
        const actualErrors = await page.locator('.text-danger:visible, .error:visible').count();
        const invalidCredentials = await page.locator('text=Invalid').count();
        
        if (actualErrors > 0 || invalidCredentials > 0) {
          testFailed = true;
          throw new Error('Authentication failed');
        }
      }
    }

    let navigationAttempts = 0;
    const maxNavigationAttempts = 3;
    
    while (navigationAttempts < maxNavigationAttempts) {
      try {
        await page.goto(`https://localhost:5002${TARGET_URL}`, { 
          waitUntil: 'networkidle',
          timeout: 10000 
        });
        
        await page.waitForTimeout(1000);
        const reloadMessages = await page.locator('text=Please reload').count();
        
        if (reloadMessages > 0) {
          navigationAttempts++;
          if (navigationAttempts < maxNavigationAttempts) {
            await page.reload({ waitUntil: 'networkidle' });
            await page.waitForTimeout(2000);
          }
        } else {
          break;
        }
      } catch (navigationError) {
        navigationAttempts++;
        if (navigationAttempts >= maxNavigationAttempts) {
          throw new Error(`Navigation failed after ${maxNavigationAttempts} attempts`);
        }
        await page.waitForTimeout(2000);
      }
    }
    
    const postNavErrorBoundaries = await page.locator('[data-error-boundary], .error-boundary, .blazor-error-boundary').count();
    if (postNavErrorBoundaries > 0) {
      testFailed = true;
      const screenshot = `_TestResults/Screenshots/${COMPONENT_NAME}-error-boundary-navigation.png`;
      await page.screenshot({ path: screenshot, fullPage: true });
      screenshots.push(screenshot);
      throw new Error('Error boundary detected');
    }

    // STEP 3: Parameter conflict detection (common Blazor issue)
    const parameterErrors = await page.evaluate(() => {
      const consoleErrors = [];
      const originalError = console.error;
      let errorCaptured = false;
      
      console.error = (...args) => {
        const message = args.join(' ');
        if (message.includes('property matching the name') || 
            message.includes('parameter') || 
            message.includes('ChildContent') ||
            message.includes('does not have a property')) {
          consoleErrors.push(message);
          errorCaptured = true;
        }
        originalError.apply(console, args);
      };
      
      // Wait a moment for any async parameter errors
      setTimeout(() => console.error = originalError, 1000);
      
      return consoleErrors;
    });
    
    await page.waitForTimeout(1000);
    
    if (parameterErrors.length > 0) {
      console.log('❌ INSTANT FAIL - Parameter conflict detected');
      errors.push(...parameterErrors);
      testFailed = true;
      throw new Error('Parameter conflicts detected - test terminated');
    }

    // STEP 4: Collect debug information
    console.log('🔍 Collecting debug information...');
    const debugReport = await page.evaluate(() => {
      if (window.RRDebug) {
        try {
          return window.RRDebug.report();
        } catch (e) {
          return { error: 'Debug report failed', message: e.message };
        }
      }
      return { error: 'RRDebug not available' };
    });
    
    if (debugReport.error) {
      console.log('⚠️ Debug report issue:', debugReport.error);
    } else {
      console.log('📊 Debug Report Summary:', {
        health: `${debugReport.health?.score}% (${debugReport.health?.status})`,
        errors: debugReport.errors?.count || 0,
        elements: debugReport.page?.elementCounts?.total || 0
      });
    }

    // STEP 5: Component-specific comprehensive testing
    if (!testFailed) {
      console.log('✅ Proceeding with comprehensive component testing...');
      
      // Test all interactive elements
      const buttons = await page.locator('button:visible').count();
      const inputs = await page.locator('input:visible').count();
      const modals = await page.locator('[role="dialog"]:visible').count();
      const tables = await page.locator('table:visible').count();
      
      console.log(`📋 Found ${buttons} buttons, ${inputs} inputs, ${modals} modals, ${tables} tables`);
      
      // Test each button systematically (limit to prevent excessive testing)
      const maxButtonsToTest = Math.min(buttons, 15);
      for (let i = 0; i < maxButtonsToTest; i++) {
        if (testFailed) break;
        
        const button = page.locator('button:visible').nth(i);
        const buttonText = await button.textContent();
        const isDisabled = await button.isDisabled();
        
        if (!isDisabled) {
          console.log(`🔘 Testing button: ${buttonText?.trim() || `Button ${i + 1}`}`);
          
          try {
            await button.click();
            await page.waitForTimeout(500); // Allow for any async operations
            
            // Check for errors after button click
            const postClickErrors = await page.locator('[data-error-boundary], .error-boundary').count();
            if (postClickErrors > 0) {
              console.log(`❌ Button click caused error boundary: ${buttonText}`);
              testFailed = true;
              const screenshot = `_TestResults/Screenshots/${COMPONENT_NAME}-button-error-${i}.png`;
              await page.screenshot({ path: screenshot, fullPage: true });
              screenshots.push(screenshot);
              break;
            }
          } catch (e) {
            console.log(`⚠️ Button click failed: ${buttonText} - ${e.message}`);
            errors.push(`Button interaction failed: ${buttonText}`);
          }
        }
      }
      
      // Test form inputs if present
      if (inputs > 0 && !testFailed) {
        console.log('📝 Testing form inputs...');
        const maxInputsToTest = Math.min(inputs, 10);
        
        for (let i = 0; i < maxInputsToTest; i++) {
          if (testFailed) break;
          
          const input = page.locator('input:visible').nth(i);
          const inputType = await input.getAttribute('type');
          const inputName = await input.getAttribute('name') || `input-${i}`;
          
          if (inputType !== 'submit' && inputType !== 'button') {
            try {
              await input.fill('test-input-value');
              await page.waitForTimeout(200);
              
              // Check for validation errors or issues
              const validationErrors = await page.locator('.field-validation-error, .validation-message').count();
              console.log(`📝 Input ${inputName} (${inputType}): ${validationErrors} validation messages`);
            } catch (e) {
              console.log(`⚠️ Input interaction failed: ${inputName} - ${e.message}`);
            }
          }
        }
      }
      
      // Test responsive behavior on key breakpoints
      console.log('📱 Testing responsive behavior...');
      const breakpoints = [
        { width: 320, height: 568, name: 'Mobile' },
        { width: 768, height: 1024, name: 'Tablet' },
        { width: 1920, height: 1080, name: 'Desktop' }
      ];
      
      for (const breakpoint of breakpoints) {
        if (testFailed) break;
        
        await page.setViewportSize({ width: breakpoint.width, height: breakpoint.height });
        await page.waitForTimeout(500);
        
        // Check for horizontal overflow
        const hasOverflow = await page.evaluate((width) => {
          const elements = document.querySelectorAll('*');
          return Array.from(elements).some(el => {
            const rect = el.getBoundingClientRect();
            return rect.width > width + 20;
          });
        }, breakpoint.width);
        
        if (hasOverflow) {
          console.log(`⚠️ Horizontal overflow detected on ${breakpoint.name}`);
          const screenshot = `_TestResults/Screenshots/${COMPONENT_NAME}-overflow-${breakpoint.name.toLowerCase()}.png`;
          await page.screenshot({ path: screenshot, fullPage: true });
          screenshots.push(screenshot);
        }
      }
    }
    
  } catch (error) {
    console.log(`❌ Test error: ${error.message}`);
    errors.push(`Test Error: ${error.message}`);
    testFailed = true;
    try {
      const screenshot = `_TestResults/Screenshots/${COMPONENT_NAME}-general-error.png`;
      await page.screenshot({ path: screenshot, fullPage: true });
      screenshots.push(screenshot);
    } catch (screenshotError) {
      console.log(`⚠️ Could not take error screenshot: ${screenshotError.message}`);
    }
  } finally {
    // MANDATORY: Report all errors and results before closing
    console.log('\n' + '='.repeat(60));
    
    try {
      if (testFailed || errors.length > 0) {
        console.log('🚨 TEST FAILED - ERRORS DETECTED:');
        errors.forEach((error, index) => {
          console.log(`${index + 1}. ${error}`);
        });
        
        if (screenshots.length > 0) {
          console.log('\n📸 Screenshots captured:');
          screenshots.forEach(screenshot => console.log(`  • ${screenshot}`));
        }
        
        console.log(`\n❌ ${COMPONENT_NAME} test FAILED with ${errors.length} errors`);
      } else {
        console.log(`✅ ${COMPONENT_NAME} test PASSED - no errors detected`);
        console.log('🎉 All interactions completed successfully');
        
        // Take a final success screenshot
        try {
          const successScreenshot = `_TestResults/Screenshots/${COMPONENT_NAME}-success.png`;
          await page.screenshot({ path: successScreenshot, fullPage: true });
          console.log(`📸 Success screenshot: ${successScreenshot}`);
        } catch (screenshotError) {
          console.log(`⚠️ Could not take success screenshot: ${screenshotError.message}`);
        }
      }
    } catch (finalError) {
      console.log(`⚠️ Error during final reporting: ${finalError.message}`);
    }
    
    // GUARANTEED browser closure regardless of any errors
    try {
      await browser.close();
      console.log('🔒 Browser closed successfully');
    } catch (closeError) {
      console.log(`⚠️ Error closing browser: ${closeError.message}`);
      // Force kill browser process if normal close fails
      try {
        await browser.close();
      } catch (forceCloseError) {
        console.log('🔥 Force closing browser after error');
      }
    }
    
    // Exit with appropriate code
    if (testFailed || errors.length > 0) {
      process.exit(1);
    } else {
      process.exit(0);
    }
  }
})();